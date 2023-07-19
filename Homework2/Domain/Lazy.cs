namespace Fuse8_ByteMinds.SummerSchool.Domain;

/// <summary>
/// Контейнер для значения, с отложенным получением
/// </summary>
public class Lazy<TValue>
{
    private readonly Func<TValue?> _creationFunc;

    private TValue? _value;

    private bool _isCreated;

    public Lazy(Func<TValue?> creationFunc)
    {
        this._creationFunc = creationFunc;
    }


    public TValue? Value
    {
        get
        {
            if (this._isCreated)
            {
                return this._value;
            }

            this._isCreated = true;
            return this._value = this._creationFunc();
        }
    }
}
