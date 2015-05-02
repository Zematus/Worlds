using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate float EvaluationDelegate (OptionEvaluator evaluator);
public delegate void ExecutionDelegate (OptionEvaluator evaluator);
public delegate void FinalizationDelegate (OptionEvaluator evaluator);
public delegate void ResetDelegate (OptionEvaluator evaluator);

public abstract class EvaluationContextElement {
	
	public string Name { get; private set; }
	
	public EvaluationContextElement (string name) {
		
		Name = name;
	}
}

public class EvaluationContextElementHolder {
	
	public Dictionary<string, EvaluationContextElement> EvaluationContext { get; private set; }

	public EvaluationContextElementHolder () {
		
		EvaluationContext = new Dictionary<string, EvaluationContextElement>();
	}
	
	public void AddContextElement (EvaluationContextElement element) {
		
		EvaluationContext.Add(element.Name, element);
	}
	
	public bool TryGetContextElement (string key, out EvaluationContextElement element) {
		
		return EvaluationContext.TryGetValue(key, out element);
	}
}

public class OptionEvaluator : EvaluationContextElementHolder {

	public string Name { get; private set; }
	
	public float LastValue { get; private set; }

	public bool Evaluated { get; private set; }

	private List<string> _evaluationLog = new List<string>();
	private List<string> _executionLog = new List<string>();

	private EvaluationDelegate _evaluationMethod;
	private ExecutionDelegate _executionMethod;

	public OptionEvaluator (string optionName, EvaluationDelegate evaluationMethod, ExecutionDelegate executionMethod = null) {

		if (string.IsNullOrEmpty(optionName)) throw new System.ArgumentNullException("'id' can't be null or empty");
		if (evaluationMethod == null) throw new System.ArgumentNullException("'evaluationMethod' can't be null");

		Name = optionName;

		Evaluated = false;

		_evaluationMethod = evaluationMethod;
		_executionMethod = executionMethod;
	}

	public float Evaluate () {
		
		EvaluationContext.Clear();

		LastValue = _evaluationMethod(this);

		Evaluated = true;

		return LastValue;
	}

	public void EvaluationLog (string comment) {
	
		_evaluationLog.Add(comment);
	}
	
	public void ExecutionLog (string comment) {
		
		_executionLog.Add(comment);
	}
	
	private void ClearLogs () {
		
		_evaluationLog.Clear();
		_executionLog.Clear();
	}

	public List<string> GetEvaluationLog () {
	
		return _evaluationLog;
	}
	
	public List<string> GetExecutionLog () {
		
		return _executionLog;
	}

	public void ExecuteOption () {
	
		if (_executionMethod == null) return;

		_executionMethod(this);
	}

	public void Reset () {
		
		ClearLogs();

		Evaluated = false;
	}
}

public class OptionSelector : IEnumerable<OptionEvaluator> {

	public bool EvaluatedOptions { get; private set; }
	public bool ExecutedSelectedOption { get; private set; }

	public List<OptionEvaluator> OptionEvaluators { get; private set; }

	public OptionEvaluator SelectedEvaluator { get; private set; }
	
	private float _bestValue = float.MinValue;

	private FinalizationDelegate _finalizationMethod = null;
	private ResetDelegate _resetMethod = null;

	public OptionSelector () {

		OptionEvaluators = new List<OptionEvaluator>();

		SelectedEvaluator = null;

		EvaluatedOptions = false;
		ExecutedSelectedOption = false;
	}

	public void SetFinalizationMethod (FinalizationDelegate method) {
	
		_finalizationMethod = method;
	}
	
	public void SetResetMethod (ResetDelegate method) {
		
		_resetMethod = method;
	}

	public void Reset () {

		if (_resetMethod != null) _resetMethod(SelectedEvaluator);

		foreach (OptionEvaluator evaluator in OptionEvaluators)
		{
			evaluator.Reset();
		}
		
		_bestValue = float.MinValue;
		SelectedEvaluator = null;
		EvaluatedOptions = false;
		ExecutedSelectedOption = false;
	}
	
	public void AddEvaluators (IEnumerable<OptionEvaluator> evaluators) {
		
		OptionEvaluators.AddRange(evaluators);
	}

	public void AddEvaluator (OptionEvaluator evaluator) {

		OptionEvaluators.Add(evaluator);
	}
	
	public void AddEvaluator (string optionName, EvaluationDelegate evaluationMethod, ExecutionDelegate executionMethod = null) {

		OptionEvaluator evaluator = new OptionEvaluator(optionName, evaluationMethod, executionMethod);

		OptionEvaluators.Add(evaluator);
	}

	public void RemoveEvaluator (OptionEvaluator evaluator) {
		
		OptionEvaluators.Remove(evaluator);
	}
	
	public void Clear () {
		
		Reset();
		
		OptionEvaluators.Clear();

		_finalizationMethod = null;
		_resetMethod = null;
	}

	public int EvaluatorCount {

		get {

			return OptionEvaluators.Count;
		}
	}

	public void EvaluateOptions () {

		if (EvaluatedOptions) return;

		foreach (OptionEvaluator evaluator in OptionEvaluators)
		{
			if (evaluator.Evaluated) continue;

			float value = evaluator.Evaluate();

			if (value > _bestValue)
			{
				SelectedEvaluator = evaluator;
				_bestValue = value;
			}
		}

		EvaluatedOptions = true;
	}

	public void ExecuteSelectedOption () {

		if (!EvaluatedOptions) {

			throw new System.Exception("Options must be evaluated first");
		}

		if (ExecutedSelectedOption) {

			return;
		}

		if (SelectedEvaluator == null)
			throw new System.NullReferenceException("There's no selected option.");

		SelectedEvaluator.ExecuteOption();

		ExecutedSelectedOption = true;
	}

	public void FinalizeSelection () {

		if (_finalizationMethod == null) return;

		_finalizationMethod(SelectedEvaluator);
		
		Reset();
	}

	IEnumerator<OptionEvaluator> IEnumerable<OptionEvaluator>.GetEnumerator()
	{
		return OptionEvaluators.GetEnumerator();
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return OptionEvaluators.GetEnumerator();
	}
}
