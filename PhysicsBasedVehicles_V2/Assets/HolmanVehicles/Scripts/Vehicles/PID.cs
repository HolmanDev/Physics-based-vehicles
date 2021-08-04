// Thank you Val for the help with the PID controller and so much more!
// https://github.com/Val7498
// https://www.youtube.com/channel/UCqrFzplmbGNcX04DHvhp4mQ
[System.Serializable]
public class PID {
	public float Kp, Ki, Kd;
		
	float integral;
	float lastError;
	
	public PID(float kp, float ki, float kd) {
		this.Kp = kp;
		this.Ki = ki;
		this.Kd = kd;
	}	
	
	public float Update(float currentError, float timeFrame) {
		integral += currentError * timeFrame;
		float deriv = (currentError - lastError) / timeFrame;
		lastError = currentError;
		return currentError * Kp + integral * Ki + deriv * Kd;
	}
}
