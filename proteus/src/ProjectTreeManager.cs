using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proteus
{
    public class ProjectTreeManager
    {
        public List<BuildConfiguration> BuildConfigurations = new List<BuildConfiguration>();
        public Dictionary<BuildConfiguration, ProjectTree> Trees;
        public Dictionary<BuildConfiguration, BuildTarget> DefaultTargets;

        public ProjectTreeManager()
        {
            Trees = new Dictionary<BuildConfiguration, ProjectTree>();
            DefaultTargets = new Dictionary<BuildConfiguration, BuildTarget>();
        }

        #region Public:Methods

        public BuildTarget GetDefaultTargetByConfiguration(BuildConfiguration bc)
        {
            KeyValuePair<BuildConfiguration, BuildTarget> kp = DefaultTargets.Where(x =>
                    x.Key.BuildPlatform.ToLower().Equals(bc.BuildPlatform.ToLower())
            &&
                    x.Key.ConfigurationName.ToLower().Equals(bc.ConfigurationName.ToLower())
             ).FirstOrDefault();

            return kp.Value;
        }
        public ProjectTree GetTreeByConfiguration(BuildConfiguration bc)
        {
            ProjectTree buildTree;
            //Trees.TryGetValue(bc, out buildTree);
            KeyValuePair<BuildConfiguration,ProjectTree> kp = Trees.Where(x =>
                                x.Key.BuildPlatform.ToLower().Equals(bc.BuildPlatform.ToLower())
                        &&
                                x.Key.ConfigurationName.ToLower().Equals(bc.ConfigurationName.ToLower())
                ).FirstOrDefault();
            buildTree = kp.Value;
            return buildTree;
        }
        public void BuildProjectDependencyTree(List<BuildTarget> targets)
        {
            //First get all possible build configurations AND parse defaults.
            ParseDefaultsAndGatherBuildConfigurations(ref targets);
            ApplyProjectDefaults(ref targets);
            BuildProjectDependencyTree(ref targets);
        }
        public void AddDependency(BuildConfiguration bc, BuildTarget a, BuildTarget b)
        {
            // A depends on B.
            // If B is NULL insert A at root
            ProjectTree tree = null;

            if (Trees.TryGetValue(bc, out tree) == false)
                throw new Exception("[BuildTreeMan] Could not get build tree for the given configuration " + bc.ToString());

            ItemNode<BuildTarget> ni = tree.Find(a);
            if (ni == null)
                // add at root
                ni = tree.Insert(a, null);

            if (b != null)
            {
                if (tree.Find(b, ni) != null)
                    throw new Exception("[BuildTreeMan] Tried to insert a node that would cause a recursive dependency. ");

                //Make sure there are no dupes.
                //Remove node before adding.  This is so we can add something with a null
                tree.Remove(b);

                tree.Insert(b, a);
            }
        }

        #endregion

        #region Private: Methods

        private void ParseDefaultsAndGatherBuildConfigurations(ref List<BuildTarget> targets)
        {
            foreach (BuildTarget objTaret in targets)
            {
                BuildConfiguration found = BuildConfigurations.Where(
                    x =>
                           (x.BuildPlatform == objTaret.Platform)
                        && (
                            x.ConfigurationName.ToLower().Equals(objTaret.ConfigurationName.ToLower())
                        )

                        ).FirstOrDefault();

                if (found == null)
                {
                    found = new BuildConfiguration(objTaret.ConfigurationName, objTaret.Platform);
                    BuildConfigurations.Add(found);
                    Trees.Add(found, new ProjectTree());
                }

                objTaret.BuildConfiguration = found;

                if (objTaret.TargetName.ToLower().Equals(BuildUtils.DefaultTargetName.ToLower()))
                {
                    DefaultTargets.Add(found, objTaret);
                }
            }

        }
        private void ApplyProjectDefaults(ref List<BuildTarget> targets)
        {
            //Remove defaults from the main list.
            foreach (BuildTarget val in DefaultTargets.Values)
            {
                targets.Remove(val);
            }

            // - Apply defaults
            foreach (BuildTarget bt in targets)
            {
                if (bt.InheritDefaults == true)
                {
                    BuildTarget def = GetDefaultTargetByConfiguration(bt.BuildConfiguration);

                    bt.AddFrom(def);
                }
            }

        }
        private void BuildProjectDependencyTree(ref List<BuildTarget> targets)
        {

            // Next add dependencies.
            Globals.Logger.LogInfo("Building project dependency tree..");
            foreach (BuildTarget objTaret in targets)
            {
                //Force add the object at root in case it has no deps.
                //If it is a dependency then we will remove it first
                AddDependency(objTaret.BuildConfiguration, objTaret, null);

                foreach (string dep in objTaret.ProjectDependencies)
                {
                    BuildTarget bt = targets.Where(
                        x => (x.TargetName.ToLower().Equals(dep.ToLower()))
                        && x.BuildConfiguration == objTaret.BuildConfiguration
                        ).FirstOrDefault();
                    if (bt == null)
                        throw new Exception("[BuildTreeMan] Could not find project dependency '" + dep + "' for target '" + objTaret.TargetName + "' with config '" + objTaret.BuildConfiguration.ToString() + "'.");

                    AddDependency(objTaret.BuildConfiguration, objTaret, bt);
                }
            }
        }
        #endregion
    }
}
