using Fovero.Model.Generators;
using Fovero.Model.Presentation;
using Fovero.Model.Solvers;
using Fovero.Model.Tiling;
using MoreLinq;

namespace Fovero.Tests
{
    internal class MazeFixture
    {
        [TestCaseSource(nameof(TestDataForVerifyScenario))]
        public void VerifyScenario(ITiling tiling, BuildingStrategy<SharedBorder> buildingStrategy, SolvingStrategy solvingStrategy, string expectedSolution)
        {
            var random = new Random(12345);
            var maze = new Maze(tiling);
            var trailMap = maze.CreateTrailMap();

            buildingStrategy
                .SelectBordersToBeOpened(maze.SharedBorders.ToList(), random)
                .ForEach(x => x.IsOpen = true);

            trailMap
                .EnumerateSolutionSteps(solvingStrategy)
                .ForEach(action => action.Invoke());

            Assert.That(trailMap.IsSolved, Is.True);

            var actualSolution = string.Join(", ", trailMap.Solution.Select(x => x.Ordinal));

            Assert.That(actualSolution, Is.EqualTo(expectedSolution));
            // TestContext.WriteLine(actualSolution);
        }

        private static IEnumerable TestDataForVerifyScenario
        {
            get
            {
                var solutions = new (BuildingStrategy<SharedBorder> Builder, string Solution)[]
                {
                    (BuildingStrategy<SharedBorder>.HuntAndKill, "0, 8, 16, 24, 32, 33, 41, 49, 48, 56, 57, 58, 59, 60, 61, 62, 63"),
                    (BuildingStrategy<SharedBorder>.Kruskal, "0, 1, 9, 17, 16, 24, 32, 33, 41, 49, 50, 51, 43, 44, 52, 53, 54, 55, 63"),
                    (BuildingStrategy<SharedBorder>.Prim, "0, 1, 9, 10, 11, 12, 13, 21, 22, 23, 31, 39, 47, 55, 63"),
                    (BuildingStrategy<SharedBorder>.RecursiveBacktracker, "0, 8, 16, 24, 32, 33, 41, 49, 48, 56, 57, 58, 59, 60, 61, 62, 63"),
                    (BuildingStrategy<SharedBorder>.Wilson, "0, 8, 9, 17, 25, 26, 27, 19, 20, 28, 36, 44, 45, 37, 38, 39, 47, 46, 54, 53, 61, 62, 63")
                };

                return solutions.Cartesian(SolvingStrategy.All, (build, solver) => new TestCaseData(new SquareTiling(8, 8), build.Builder, solver, build.Solution)
                {
                    TestName = $"{build.Builder.Name}: {solver.Name}"
                });
            }
        }
    }
}
