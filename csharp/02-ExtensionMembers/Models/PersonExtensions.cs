namespace ExtensionBlocks.Models;

/// <summary>
/// C# 14 Extension Block for Person class demonstrating extension properties.
/// Extension properties provide a clean way to add computed properties to existing types.
/// 
/// Uses the actual C# 14 extension syntax that compiles and runs in .NET 10.
/// This provides true extension properties that are accessed as person.FullName
/// (not person.FullName()) and offers better performance and IntelliSense support.
/// </summary>
public static class PersonExtensions
{
    extension (Person person)
    {
        /// <summary>
        /// Extension property that calculates the person's full name.
        /// Accessed as a true property: person.FullName (not person.FullName())
        /// </summary>
        public string FullName => $"{person.FirstName} {person.LastName}";

        /// <summary>
        /// Extension property that calculates the person's age in years.
        /// Demonstrates computed properties with complex logic in extension members.
        /// Accessed as a true property: person.Age (not person.Age())
        /// </summary>
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - person.DateOfBirth.Year;
                if (person.DateOfBirth.Date > today.AddYears(-age))
                    age--;
                return age;
            }
        }

        /// <summary>
        /// Extension property that calculates Body Mass Index (BMI).
        /// Shows how extension properties can perform calculations using multiple base properties.
        /// Accessed as a true property: person.BMI (not person.BMI())
        /// </summary>
        public double BMI => person.WeightInKg / (person.HeightInMeters * person.HeightInMeters);

        /// <summary>
        /// Extension property that categorizes BMI into health categories.
        /// Demonstrates extension properties with conditional logic.
        /// Accessed as a true property: person.BMICategory (not person.BMICategory())
        /// </summary>
        public string BMICategory => person.BMI switch
        {
            < 18.5 => "Underweight",
            >= 18.5 and < 25.0 => "Normal weight",
            >= 25.0 and < 30.0 => "Overweight",
            >= 30.0 => "Obese",
            _ => "Unknown"
        };

        /// <summary>
        /// Extension property that checks if the person is an adult.
        /// Simple boolean extension property demonstrating different return types.
        /// Accessed as a true property: person.IsAdult (not person.IsAdult())
        /// </summary>
        public bool IsAdult => person.Age >= 18;

        /// <summary>
        /// Extension indexer that returns person values by key.
        /// Demonstrates C# 15 extension indexers in extension blocks.
        /// </summary>
        /// <param name="fieldName">The field key to read.</param>
        /// <exception cref="ArgumentException">Thrown when the field name is not supported.</exception>
        public string this[string fieldName] => fieldName.ToLowerInvariant() switch
        {
            "firstname" => person.FirstName,
            "lastname" => person.LastName,
            "fullname" => person.FullName,
            "age" => person.Age.ToString(),
            "bmi" => person.BMI.ToString("F1"),
            "bmicategory" => person.BMICategory,
            "isadult" => person.IsAdult.ToString(),
            _ => throw new ArgumentException($"Unsupported person field '{fieldName}'.", nameof(fieldName))
        };

        /// <summary>
        /// Extension property that formats the person's vital statistics.
        /// Shows how extension properties can combine multiple other extension properties.
        /// Accessed as a true property: person.VitalStats (not person.VitalStats())
        /// </summary>
        public string VitalStats => $"{person.FullName}: {person.Age} years old, BMI: {person.BMI:F1} ({person.BMICategory})";
    }
}