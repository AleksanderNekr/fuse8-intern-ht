using System.Reflection;

namespace Fuse8_ByteMinds.SummerSchool.Domain;

public static class AssemblyHelpers
{
    private const string LookupNamespace = "Fuse8_ByteMinds.SummerSchool.Domain";

    /// <summary>
    /// Получает информацию о базовых типах классов из namespace "Fuse8_ByteMinds.SummerSchool.Domain", у которых есть наследники.
    /// </summary>
    /// <remarks>
    ///	Информация возвращается только по самым базовым классам.
    /// Информация о промежуточных базовых классах не возвращается
    /// </remarks>
    /// <returns>Список типов с количеством наследников</returns>
    public static (string BaseTypeName, int InheritorCount)[] GetTypesWithInheritors()
    {
        // Получаем все классы из текущей Assembly
        IEnumerable<TypeInfo> assemblyClassTypes = Assembly.GetAssembly(typeof(AssemblyHelpers))!.DefinedTypes
                                                           .Where(static p => p.IsClass)
                                                           .ToList(); // Во избежание повторной енумерации

        // Поиск базовых классов
        Dictionary<string, int> baseTypeChildrenCountPairs = assemblyClassTypes
                                                            .Select(static type => GetBaseType(type))
                                                            .Distinct()
                                                            .Where(static baseType => baseType is not null
                                                                    && baseType.Namespace == LookupNamespace)
                                                            .ToDictionary(keySelector: static baseType => baseType!.Name,
                                                                          elementSelector: static _ => 0);
        // Подсчет неабстрактных наследников.
        foreach (TypeInfo type in assemblyClassTypes.Where(static type => !type.IsAbstract))
        {
            Type? baseClass = GetBaseType(type);

            // Фильтрация базового класса по наличию в найденных
            if (baseClass is null || !baseTypeChildrenCountPairs.ContainsKey(baseClass.Name))
            {
                continue;
            }

            baseTypeChildrenCountPairs[baseClass.Name]++;
        }

        return baseTypeChildrenCountPairs.Select(static pair => (pair.Key, pair.Value))
                                         .ToArray();
    }

    /// <summary>
    /// Получает базовый тип для класса
    /// </summary>
    /// <param name="type">Тип, для которого необходимо получить базовый тип</param>
    /// <returns>
    /// Первый тип в цепочке наследований. Если наследования нет, возвращает null
    /// </returns>
    /// <example>
    /// Класс A, наследуется от B, B наследуется от C
    /// При вызове GetBaseType(typeof(A)) вернется C
    /// При вызове GetBaseType(typeof(B)) вернется C
    /// При вызове GetBaseType(typeof(C)) вернется C
    /// </example>
    private static Type? GetBaseType(Type type)
    {
        var baseType = type;

        while (baseType.BaseType is not null && baseType.BaseType != typeof(object))
        {
            baseType = baseType.BaseType;
        }

        return baseType == type
                   ? null
                   : baseType;
    }
}
