using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab4_Var2
{
    public class BookRecord
    {
        private string _author = string.Empty;
        private string _title = string.Empty;
        private string _publisher = string.Empty;
        private int _year;

        public BookRecord()
        {
        }

        public BookRecord(string author, string title, string publisher, int year)
        {
            Author = author;
            Title = title;
            Publisher = publisher;
            Year = year;
        }

        public string Author
        {
            get => _author;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Автор книги не може бути порожнім.");
                _author = value.Trim();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва книги не може бути порожньою.");
                _title = value.Trim();
            }
        }

        public string Publisher
        {
            get => _publisher;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Видавництво не може бути порожнім.");
                _publisher = value.Trim();
            }
        }

        public int Year
        {
            get => _year;
            set
            {
                int maxYear = DateTime.Now.Year + 1;
                if (value < 1450 || value > maxYear)
                    throw new ArgumentOutOfRangeException(nameof(Year), $"Рік видання має бути в межах 1450..{maxYear}.");
                _year = value;
            }
        }

        public bool IsProgrammingBook()
        {
            return _title.IndexOf("програмування", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class PhoneEntry
    {
        private string _lastName = string.Empty;
        private string _firstName = string.Empty;
        private string _middleName = string.Empty;
        private string _address = string.Empty;
        private string _phone = string.Empty;

        public PhoneEntry()
        {
        }

        public PhoneEntry(string lastName, string firstName, string middleName, string address, string phone)
        {
            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            Address = address;
            Phone = phone;
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Прізвище не може бути порожнім.");
                _lastName = value.Trim();
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ім'я не може бути порожнім.");
                _firstName = value.Trim();
            }
        }

        public string MiddleName
        {
            get => _middleName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("По батькові не може бути порожнім.");
                _middleName = value.Trim();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Адреса не може бути порожньою.");
                _address = value.Trim();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Номер телефону не може бути порожнім.");

                string trimmed = value.Trim();
                int digitsCount = trimmed.Count(char.IsDigit);
                bool validChars = trimmed.All(ch => char.IsDigit(ch) || ch is '+' or '-' or ' ' or '(' or ')');

                if (!validChars || digitsCount < 7 || digitsCount > 15)
                    throw new FormatException("Некоректний формат номера телефону (очікується від 7 до 15 цифр).");

                _phone = trimmed;
            }
        }

        public string ToFileLine()
        {
            return string.Join("|", LastName, FirstName, MiddleName, Address, Phone);
        }

        public static PhoneEntry FromFileLine(string line)
        {
            string[] parts = line.Split('|');
            if (parts.Length != 5)
                throw new FormatException($"Некоректний рядок бази даних: {line}");

            return new PhoneEntry(parts[0], parts[1], parts[2], parts[3], parts[4]);
        }
    }

    public class PhoneDirectoryManager
    {
        private readonly string _filePath;
        private readonly List<PhoneEntry> _entries = new List<PhoneEntry>();

        public PhoneDirectoryManager(string filePath)
        {
            _filePath = filePath;
        }

        public int Count => _entries.Count;

        public IReadOnlyList<PhoneEntry> Entries => _entries.AsReadOnly();

        public void LoadOrSeedDefault()
        {
            if (!File.Exists(_filePath))
            {
                SeedDefaultEntries();
                SaveToFile();
                return;
            }

            string[] lines = File.ReadAllLines(_filePath, Encoding.UTF8)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            if (lines.Length == 0)
            {
                SeedDefaultEntries();
                SaveToFile();
                return;
            }

            _entries.Clear();
            foreach (string line in lines)
            {
                _entries.Add(PhoneEntry.FromFileLine(line));
            }
        }

        public void SaveToFile()
        {
            var lines = _entries.Select(e => e.ToFileLine());
            File.WriteAllLines(_filePath, lines, Encoding.UTF8);
        }

        public void AddEntry(PhoneEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            _entries.Add(entry);
            SaveToFile();
        }

        public void UpdateEntry(int index, PhoneEntry updated)
        {
            if (index < 0 || index >= _entries.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Невірний номер запису.");

            _entries[index] = updated;
            SaveToFile();
        }

        public void DeleteEntry(int index)
        {
            if (index < 0 || index >= _entries.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Невірний номер запису.");

            _entries.RemoveAt(index);
            SaveToFile();
        }

        public List<PhoneEntry> SearchByLastName(string lastNameQuery)
        {
            if (string.IsNullOrWhiteSpace(lastNameQuery))
                throw new ArgumentException("Рядок пошуку за прізвищем не може бути порожнім.");

            string q = lastNameQuery.Trim();
            return _entries
                .Where(e => e.LastName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        public void SortByPhone()
        {
            _entries.Sort((a, b) =>
            {
                string digitsA = new string(a.Phone.Where(char.IsDigit).ToArray());
                string digitsB = new string(b.Phone.Where(char.IsDigit).ToArray());
                int cmp = string.Compare(digitsA, digitsB, StringComparison.Ordinal);
                if (cmp != 0)
                    return cmp;
                return string.Compare(a.LastName, b.LastName, StringComparison.CurrentCultureIgnoreCase);
            });
            SaveToFile();
        }

        private void SeedDefaultEntries()
        {
            _entries.Clear();
            _entries.AddRange(new[]
            {
                new PhoneEntry("Мельничук", "Нікіта", "Сергійович", "м. Чернівці, вул. Головна, 102", "+380501112233"),
                new PhoneEntry("Коваленко", "Олена", "Іванівна", "м. Чернівці, вул. Героїв Майдану, 45", "+380672223344"),
                new PhoneEntry("Шевченко", "Андрій", "Петрович", "м. Київ, вул. Хрещатик, 18", "+380633334455"),
                new PhoneEntry("Бондар", "Марія", "Олегівна", "м. Львів, вул. Франка, 27", "+380954445566"),
                new PhoneEntry("Ткаченко", "Дмитро", "Васильович", "м. Чернівці, вул. Руська, 89", "+380665556677"),
                new PhoneEntry("Кравченко", "Ірина", "Миколаївна", "м. Івано-Франківськ, вул. Незалежності, 12", "+380976667788"),
                new PhoneEntry("Мороз", "Олександр", "Вікторович", "м. Чернівці, вул. Комарова, 14", "+380507778899"),
                new PhoneEntry("Савченко", "Наталія", "Юріївна", "м. Вінниця, вул. Соборна, 60", "+380688889900"),
                new PhoneEntry("Поліщук", "Максим", "Романович", "м. Чернівці, вул. Садова, 8", "+380939990011"),
                new PhoneEntry("Гончар", "Софія", "Андріївна", "м. Тернопіль, вул. Руська, 34", "+380501234567")
            });
        }
    }

    internal class Program
    {
        private static readonly List<BookRecord> Books = new List<BookRecord>
        {
            new BookRecord("Роберт Мартін", "Чистий код: створення, аналіз та рефакторинг", "Фабула", 2019),
            new BookRecord("Джеффрі Ріхтер", "CLR via C#. Програмування на платформі .NET Framework", "BHV", 2016),
            new BookRecord("Томас Кормен", "Алгоритми: побудова та аналіз", "К.І.С.", 2019),
            new BookRecord("Джон Скіт", "C# для професіоналів. Програмування на C#", "Вільямс", 2021),
            new BookRecord("Ліна Костенко", "Маруся Чурай", "Либідь", 2015),
            new BookRecord("Ендрю Троелсен", "Мова програмування C# 10 та платформа .NET 6", "Вільямс", 2022),
            new BookRecord("Марк Сіманн", "Впровадження залежностей у .NET", "Пітер", 2020),
            new BookRecord("Браян Керніган", "Мова програмування C", "Діалектика", 2018),
            new BookRecord("Іван Франко", "Захар Беркут", "Основи", 2017),
            new BookRecord("Роберт Седжвік", "Алгоритми на C#", "Вільямс", 2023)
        };

        private static void Main()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            string dbPath = Path.Combine(AppContext.BaseDirectory, "phonebook_db.txt");
            var directory = new PhoneDirectoryManager(dbPath);

            try
            {
                directory.LoadOrSeedDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження бази даних: {ex.Message}");
                return;
            }

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("==================================================================");
                Console.WriteLine("          ЛАБОРАТОРНА РОБОТА №4 (ВАРІАНТ 2) — КЛАСИ ТА ООП");
                Console.WriteLine("==================================================================");
                Console.WriteLine("  [1] - Завдання 1: Книги з програмування (спадання року видання)");
                Console.WriteLine("  [Д] - Завдання 2: Додати запис до телефонного довідника");
                Console.WriteLine("  [Р] - Завдання 2: Редагувати запис телефонного довідника");
                Console.WriteLine("  [З] - Завдання 2: Знищити (видалити) запис");
                Console.WriteLine("  [В] - Завдання 2: Вивести записи телефонного довідника на екран");
                Console.WriteLine("  [П] - Завдання 2: Пошук за прізвищем");
                Console.WriteLine("  [С] - Завдання 2: Сортування за телефоном");
                Console.WriteLine("  [Enter] - Вихід з програми");
                Console.Write("Оберіть клавішу дії: ");

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                Console.WriteLine(keyInfo.KeyChar == '\0' ? keyInfo.Key.ToString() : keyInfo.KeyChar.ToString());

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("Роботу завершено.");
                    break;
                }

                char ch = char.ToUpperInvariant(keyInfo.KeyChar);

                try
                {
                    switch (ch)
                    {
                        case '1':
                            RunTask1ProgrammingBooks();
                            break;
                        case 'Д':
                        case 'A':
                            AddPhoneEntryInteractive(directory);
                            break;
                        case 'Р':
                        case 'E':
                            EditPhoneEntryInteractive(directory);
                            break;
                        case 'З':
                        case 'D':
                            DeletePhoneEntryInteractive(directory);
                            break;
                        case 'В':
                        case 'L':
                            PrintPhoneEntries(directory.Entries, "Телефонний довідник");
                            break;
                        case 'П':
                        case 'F':
                            SearchByLastNameInteractive(directory);
                            break;
                        case 'С':
                        case 'S':
                            directory.SortByPhone();
                            Console.WriteLine("Записи відсортовано за полем «Телефон» та збережено у файл.");
                            PrintPhoneEntries(directory.Entries, "Телефонний довідник (відсортовано за телефоном)");
                            break;
                        default:
                            Console.WriteLine("Невідома клавіша. Натисніть [1], [Д], [Р], [З], [В], [П], [С] або [Enter] для виходу.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Помилка введення/обробки]: {ex.Message}");
                }
            }
        }

        private static void RunTask1ProgrammingBooks()
        {
            Console.WriteLine();
            Console.WriteLine("--- Завдання 1: Книги з програмування (спадання року видання) ---");

            var filtered = Books
                .Where(b => b.IsProgrammingBook())
                .OrderByDescending(b => b.Year)
                .ThenBy(b => b.Title)
                .ToList();

            Console.WriteLine($"{"Рік",-6} | {"Автор",-22} | {"Видавництво",-14} | {"Назва книги"}");
            Console.WriteLine(new string('-', 95));

            foreach (var book in filtered)
            {
                Console.WriteLine($"{book.Year,-6} | {book.Author,-22} | {book.Publisher,-14} | {book.Title}");
            }

            Console.Write("Бажаєте додати ще одну книгу для перевірки? (т/н): ");
            string? ans = Console.ReadLine();
            if (string.Equals(ans?.Trim(), "т", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("Автор: ");
                string author = Console.ReadLine() ?? string.Empty;

                Console.Write("Назва книги: ");
                string title = Console.ReadLine() ?? string.Empty;

                Console.Write("Видавництво: ");
                string publisher = Console.ReadLine() ?? string.Empty;

                Console.Write("Рік видання: ");
                string yearRaw = Console.ReadLine() ?? string.Empty;
                if (!int.TryParse(yearRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int year))
                    throw new FormatException("Рік видання повинен бути цілим числом.");

                Books.Add(new BookRecord(author, title, publisher, year));
                Console.WriteLine("Книгу додано. Оновлений список книг з програмування:");

                var updatedFiltered = Books
                    .Where(b => b.IsProgrammingBook())
                    .OrderByDescending(b => b.Year)
                    .ToList();

                Console.WriteLine($"{"Рік",-6} | {"Автор",-22} | {"Видавництво",-14} | {"Назва книги"}");
                Console.WriteLine(new string('-', 95));
                foreach (var book in updatedFiltered)
                {
                    Console.WriteLine($"{book.Year,-6} | {book.Author,-22} | {book.Publisher,-14} | {book.Title}");
                }
            }
        }

        private static void AddPhoneEntryInteractive(PhoneDirectoryManager directory)
        {
            Console.WriteLine();
            Console.WriteLine("--- Додавання нового запису ---");
            PhoneEntry entry = ReadPhoneEntryFromConsole();
            directory.AddEntry(entry);
            Console.WriteLine("Запис успішно додано та збережено у файл.");
        }

        private static void EditPhoneEntryInteractive(PhoneDirectoryManager directory)
        {
            Console.WriteLine();
            Console.WriteLine("--- Редагування запису ---");
            PrintPhoneEntries(directory.Entries, "Поточні записи");

            if (directory.Count == 0)
                return;

            Console.Write($"Введіть номер запису для редагування (1..{directory.Count}): ");
            int idx = ReadIndexInRange(1, directory.Count) - 1;

            Console.WriteLine("Введіть нові дані для запису:");
            PhoneEntry updated = ReadPhoneEntryFromConsole();
            directory.UpdateEntry(idx, updated);
            Console.WriteLine("Запис успішно оновлено та збережено у файл.");
        }

        private static void DeletePhoneEntryInteractive(PhoneDirectoryManager directory)
        {
            Console.WriteLine();
            Console.WriteLine("--- Знищення запису ---");
            PrintPhoneEntries(directory.Entries, "Поточні записи");

            if (directory.Count == 0)
                return;

            Console.Write($"Введіть номер запису для видалення (1..{directory.Count}): ");
            int idx = ReadIndexInRange(1, directory.Count) - 1;

            directory.DeleteEntry(idx);
            Console.WriteLine("Запис видалено та файл оновлено.");
        }

        private static void SearchByLastNameInteractive(PhoneDirectoryManager directory)
        {
            Console.WriteLine();
            Console.WriteLine("--- Пошук за прізвищем ---");
            Console.Write("Введіть прізвище (або його частину): ");
            string query = Console.ReadLine() ?? string.Empty;

            var found = directory.SearchByLastName(query);
            if (found.Count == 0)
            {
                Console.WriteLine("Записів за вказаним прізвищем не знайдено.");
                return;
            }

            PrintPhoneEntries(found, $"Результати пошуку за прізвищем «{query.Trim()}»");
        }

        private static PhoneEntry ReadPhoneEntryFromConsole()
        {
            Console.Write("Прізвище: ");
            string lastName = Console.ReadLine() ?? string.Empty;

            Console.Write("Ім'я: ");
            string firstName = Console.ReadLine() ?? string.Empty;

            Console.Write("По батькові: ");
            string middleName = Console.ReadLine() ?? string.Empty;

            Console.Write("Адреса: ");
            string address = Console.ReadLine() ?? string.Empty;

            Console.Write("Телефон: ");
            string phone = Console.ReadLine() ?? string.Empty;

            return new PhoneEntry(lastName, firstName, middleName, address, phone);
        }

        private static int ReadIndexInRange(int min, int max)
        {
            string raw = Console.ReadLine() ?? string.Empty;
            if (!int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                throw new FormatException("Номер запису повинен бути цілим числом.");

            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(nameof(value), $"Номер запису має бути в межах від {min} до {max}.");

            return value;
        }

        private static void PrintPhoneEntries(IReadOnlyList<PhoneEntry> entries, string header)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {header} (всього: {entries.Count}) ===");
            if (entries.Count == 0)
            {
                Console.WriteLine("Список порожній.");
                return;
            }

            Console.WriteLine($"{"№",-3} | {"Прізвище",-14} | {"Ім'я",-11} | {"По батькові",-14} | {"Телефон",-16} | {"Адреса"}");
            Console.WriteLine(new string('-', 105));

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                Console.WriteLine($"{i + 1,-3} | {e.LastName,-14} | {e.FirstName,-11} | {e.MiddleName,-14} | {e.Phone,-16} | {e.Address}");
            }
        }
    }
}
