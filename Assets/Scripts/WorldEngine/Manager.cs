using UnityEngine;
using System.Collections;

public class Manager {

	private static Manager _manager = new Manager();

	private World _currentWorld;

	public static World CurrentWorld { get {return _manager._currentWorld; }}

	public static World GenerateNewWorld () {

		int width = 200;
		int height = 100;
		int seed = Random.Range(0, int.MaxValue);

		World world = new World(width, height, seed);
		world.Generate();

		_manager._currentWorld = world;

		return world;
	}
}
