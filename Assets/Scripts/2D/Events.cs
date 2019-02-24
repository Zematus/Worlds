using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class MessageEvent : UnityEvent<string> { }

[System.Serializable]
public class SpeedChangeEvent : UnityEvent<Speed> { }

[System.Serializable]
public class ToggleEvent : UnityEvent<bool> { }

[System.Serializable]
public class ValueSetEvent : UnityEvent<float> { }

[System.Serializable]
public class PlanetViewEvent : UnityEvent<PlanetView> { }
