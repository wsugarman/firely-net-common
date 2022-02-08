﻿using Firely.Fhir.Packages;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Hl7.Fhir.Specification.Source
{
    /// <summary>Reads FHIR artifacts (Profiles, ValueSets, ...) from one or multiple FHIR packages.</summary>
    public class NpmPackageResolver : IAsyncResourceResolver, IArtifactSource
    {
        private Lazy<PackageContext> _context;
        private ModelInspector _provider;

        /// <summary>Create a new <see cref="NpmPackageResolver"/> instance to read FHIR artifacts from one or multiple FHIR packages of a specific FHIR version
        /// found in the paths passed to this function.</summary>
        /// <returns>A new <see cref="NpmPackageResolver"/> instance.</returns>
        /// <param name="provider">A <see cref="ModelInspector"/> used to parse the filecontents to FHIR resources, this is typically a <see cref="ModelInspector"/> containing the definitions of a specific FHIR version. </param>
        /// <param name="filePaths">A path to the FHIR package files.</param>
        public NpmPackageResolver(ModelInspector provider, params string[] filePaths)
        {
            _context = new Lazy<PackageContext>(() => TaskHelper.Await(() => createPackageContextAsync(filePaths)));
            _provider = provider;
        }

        private static async Task<PackageContext> createPackageContextAsync(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException($"File was not found: '{path}'.");
            }

            var scopePath = Path.Combine(Path.GetTempPath(), "package-" + Path.GetRandomFileName());
            if (!Directory.Exists(scopePath))
            {
                Directory.CreateDirectory(scopePath);
            }

            var scope = createContext(scopePath);

            foreach (var path in paths)
            {
                await installPackageFromPath(scope, path);
            }

            return scope;
        }

        private static async Task installPackageFromPath(PackageContext scope, string path)
        {
            var packageManifest = Packaging.ExtractManifestFromPackageFile(path);
            if (packageManifest is not null)
            {
                var reference = packageManifest.GetPackageReference();
                await scope.Cache.Install(reference, path);

                var dependency = new PackageDependency(reference.Name ?? "", reference.Version);
                if (reference.Found)
                {
                    var manifest = await scope.Project.ReadManifest();
                    var fhirVersion = packageManifest.GetFhirVersion();
                    if (fhirVersion is null)
                    {
                        throw new("Manifest doesn't contain a valid FHIR version");
                    }
                    manifest ??= ManifestFile.Create("temp", fhirVersion);
                    if (manifest.Name == reference.Name)
                    {
                        throw new("Skipped updating package manifest because it would cause the package to reference itself.");
                    }
                    else
                    {
                        manifest.AddDependency(dependency);
                        await scope.Project.WriteManifest(manifest);
                        await scope.Restore();
                    }
                }
            }

        }

        private Resource? toResource(string content)
        {
            try
            {
                var sourceNode = content.StartsWith("{")
                                ? FhirJsonNode.Parse(content)
                                : FhirXmlNode.Parse(content);

                return TypedSerialization.ToPoco(sourceNode, _provider) as Resource;
            }
            catch
            {
                return null;
            }
        }

        ///<inheritdoc/>
        public async Task<Resource?> ResolveByCanonicalUriAsync(string uri)
        {
            (var url, var version) = splitCanonical(uri);
            var content = await _context.Value.GetFileContentByCanonical(url, version, resolveBestCandidate: true);
            return content is null ? null : toResource(content);
        }


        private static (string url, string version) splitCanonical(string canonical)
        {
            if (canonical.EndsWith("|"))
                canonical = canonical.Substring(0, canonical.Length - 1);

            var position = canonical.LastIndexOf('|');

            return position == -1 ?
                (canonical, "")
                : (canonical.Substring(0, position), canonical.Substring(position + 1));
        }


        ///<inheritdoc/>
        public async Task<Resource?> ResolveByUriAsync(string uri)
        {
            uri.SplitLeft('/').Deconstruct(out var resource, out var id);

            if (resource == null || id is null)
                return null;

            var content = await _context.Value.GetFileContentById(resource, id);
            return content is null ? null : toResource(content);
        }


        private static PackageContext createContext(string folder, bool localCache = false)
        {
            string? cache_folder = localCache ? folder : null;
            var cache = new DiskPackageCache(cache_folder);
            var project = new FolderProject(folder);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return new PackageContext(cache, project, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        ///<inheritdoc/>
        public IEnumerable<string> ListArtifactNames()
        {
            return _context.Value.GetFileNames();
        }

        ///<inheritdoc/>
        public Stream? LoadArtifactByName(string artifactName)
        {
            var content = TaskHelper.Await(() => _context.Value.GetFileContentByFileName(artifactName));
            return content == null ? null : new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        ///<inheritdoc/>
        public Stream? LoadArtifactByPath(string artifactPath)
        {
            var content = TaskHelper.Await(() => _context.Value.GetFileContentByFilePath(artifactPath));
            return content == null ? null : new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        ///<inheritdoc/>
        public IEnumerable<string> ListResourceUris(string? filter = null)
        {
            return _context.Value.ListCanonicalUris(filter);
        }

        ///<inheritdoc/>
        public async Task<Resource?> FindCodeSystemByValueSet(string valueSetUri)
        {
            var content = await _context.Value.GetCodeSystemByValueSet(valueSetUri);
            return content is null ? null : toResource(content);
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<Resource>?> FindConceptMaps(string? sourceUri = null, string? targetUri = null)
        {
            var content = await _context.Value.GetConceptMapsBySourceAndTarget(sourceUri, targetUri);
            return content is null
                 ? null
                 : (from item in content
                    let resource = toResource(item)
                    where resource is not null
                    select resource);
        }

        ///<inheritdoc/>
        public async Task<Resource?> FindNamingSystemByUniqueId(string uniqueId)
        {
            var content = await _context.Value.GetNamingSystemByUniqueId(uniqueId);
            return content is null ? null : toResource(content);
        }
    }
}

#nullable restore
