using UnityEngine;

[System.Serializable]
public class PID {
	public float pFactor, iFactor, dFactor;
		
	float integral;
	float lastError;
	
	
	public float Update(float setpoint, float actual, float timeFrame) {
		float present = setpoint - actual;
		integral += present * timeFrame;
		float deriv = (present - lastError) / timeFrame;
		lastError = present;
		return present * pFactor + integral * iFactor + deriv * dFactor;
	}
}
    
[System.Serializable]
public class VectorPID {
    public float pFactor, iFactor, dFactor;

    Vector3 integral;
    Vector3 lastError;


    public Vector3 Update(Vector3 setpoint, Vector3 actual, float timeFrame) {
        Vector3 present = setpoint - actual;
        integral += present * timeFrame;
        Vector3 deriv = (present - lastError) / timeFrame;
        lastError = present;
        return present * pFactor + integral * iFactor + deriv * dFactor;
    }
}