using System.Net;

namespace Fuse8_ByteMinds.SummerSchool.Domain;

public static class ExceptionHandler
{
    /// <summary>
    /// Обрабатывает исключение, которое может возникнуть при выполнении <paramref name="action"/>
    /// </summary>
    /// <param name="action">Действие, которое может породить исключение</param>
    /// <returns>Сообщение об ошибке</returns>
    public static string? Handle(Action action)
    {
        try
        {
            action();
        }
        catch (NotValidKopekCountException)
        {
            return "Количество копеек должно быть больше 0 и меньше 99";
        }
        catch (NegativeRubleCountException)
        {
            return "Число рублей не может быть отрицательным";
        }
        catch (MoneyException moneyException)
        {
            return moneyException.Message;
        }
        catch (HttpRequestException httpException) when (httpException.StatusCode == HttpStatusCode.NotFound)
        {
            return "Ресурс не райден";
        }
        catch (HttpRequestException httpException)
        {
            return httpException.StatusCode.ToString();
        }
        catch (Exception)
        {
            return "Произошла непредвиденная ошибка";
        }

        return null;
    }
}

public class MoneyException : Exception
{
    public MoneyException()
    {
    }

    public MoneyException(string? message)
        : base(message)
    {
    }
}

public class NotValidKopekCountException : MoneyException
{
    public NotValidKopekCountException()
    {
    }
}

public class NegativeRubleCountException : MoneyException
{
    public NegativeRubleCountException()
    {
    }
}
