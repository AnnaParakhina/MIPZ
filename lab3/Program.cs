#region Original Operator Code 
// Duplicate calls to SendAlert
// "Don" and "John" are magic strings
// Nested if statements reduce readability
// Unnecessary toggling of the bool value
void CheckSecurity(string[] people)
{
    bool found = false;
    for (int i = 0; i < people.Length; i++)
    {
        if (!found)
        {
            if (people[i].Equals("Don"))
            {
                SendAlert();
                found = true;
            }
            if (people[i].Equals("John"))
            {
                SendAlert();
                found = true;
            }
        }
    }
}
#endregion

#region Refactored Operator Code
// Extracted suspicious names into a HashSet to avoid magic strings
// Replaced nested conditionals with guard clauses
// Removed redundant bool toggling and exited loop after first match
void CheckSecurityRefactored(string[] people)
{
    var suspiciousNames = new HashSet<string> { "Don", "John" };
    if (people.Any(person => suspiciousNames.Contains(person)))
    {
        SendAlert();
    }
}
#endregion


void SendAlert() { }

#region Original Data Code

// Lacks object-oriented principles
// Several properties are private and can't be set

enum TransportType
{
    Car,
    Plane,
    Submarine
}

class Transport
{
    private TransportType _type;
    private int _takeOffTime;
    private int _landingTime;
    private int _diveTime;
    private int _ascentTime;

    public Transport(TransportType type)
    {
        _type = type;
    }

    public int GetSpeed(int distance, int time)
    {
        if (time != 0)
        {
            switch (_type)
            {
                case TransportType.Car:
                    return distance / time;
                case TransportType.Plane:
                    return distance / (time - _takeOffTime - _landingTime);
                case TransportType.Submarine:
                    return distance / (time - _diveTime - _ascentTime);
            }
        }
        return 0;
    }
}

#endregion

#region Refactored Data Code

// Applied OOP principles: Subclasses for each transport type.
// Properties are settable via constructor to ensure immutability.
// Used polymorphism to handle different transport types.

abstract class TransportRefactored
{
    public abstract int GetSpeed(int distance, int time);
}

class Car : TransportRefactored
{
    public override int GetSpeed(int distance, int time)
    {
        return time != 0 ? distance / time : 0;
    }
}

class Plane : TransportRefactored
{
    private int _takeOffTime;
    private int _landingTime;

    public Plane(int takeOffTime, int landingTime)
    {
        _takeOffTime = takeOffTime;
        _landingTime = landingTime;
    }

    public override int GetSpeed(int distance, int time)
    {
        return time != 0 ? distance / (time - _takeOffTime - _landingTime) : 0;
    }
}

class Submarine : TransportRefactored
{
    private int _diveTime;
    private int _ascentTime;

    public Submarine(int diveTime, int ascentTime)
    {
        _diveTime = diveTime;
        _ascentTime = ascentTime;
    }

    public override int GetSpeed(int distance, int time)
    {
        return time != 0 ? distance / (time - _diveTime - _ascentTime) : 0;
    }
}

#endregion