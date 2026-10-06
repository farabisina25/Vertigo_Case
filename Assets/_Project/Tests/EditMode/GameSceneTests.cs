using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Vertigo.Wheel.Controllers.Game;
using Vertigo.Wheel.Editor.SceneBuilding;
using Vertigo.Wheel.Editor.Validation;

namespace Vertigo.Wheel.Tests
{
    public sealed class GameSceneTests
    {
        private Scene _scene;

        [SetUp]
        public void SetUp()
        {
            if (!File.Exists(GameSceneBuilder.ScenePath))
            {
                Assert.Ignore($"{GameSceneBuilder.ScenePath} has not been built yet (Vertigo > Build Game Scene).");
            }

            _scene = EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            if (_scene.IsValid())
            {
                EditorSceneManager.CloseScene(_scene, true);
            }
        }

        [Test]
        public void GameScene_FollowsUiRules()
        {
            var issues = UiHierarchyValidator.Validate(_scene);

            Assert.That(issues, Is.Empty, string.Join("\n", issues.Select(issue => issue.ToString())));
        }

        [Test]
        public void GameScene_HasSingleBootstrap()
        {
            int count = _scene.GetRootGameObjects().Sum(root => root.GetComponentsInChildren<GameBootstrap>(true).Length);

            Assert.That(count, Is.EqualTo(1));
        }
    }
}
