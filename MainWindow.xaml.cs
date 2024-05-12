using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite; // Підключення до SQLite для роботи з базою даних
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // Необхідно для сортування та фільтрації даних в WPF
using System.Windows.Documents;
using System.Windows.Media;

namespace Library
{
    // Клас, що представляє книгу
    public class Book
    {
        public int Id { get; set; } // Ідентифікатор книги
        public string Title { get; set; } // Назва книги
        public string Author { get; set; } // Автор
        public string Genre { get; set; } // Жанр
        public int PageCount { get; set; } // Кількість сторінок
    }

    // Основне вікно програми
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=books.db;Version=3;";
        private MediaPlayer successSoundPlayer = new MediaPlayer(); // Відтворювач звукових ефектів
        private MediaPlayer deleteSoundPlayer = new MediaPlayer(); // Відтворювач звукових ефектів

        public MainWindow()
        {
            InitializeComponent();
            CreateNewDatabase();
            Console.WriteLine("DB path: " + System.IO.Path.GetFullPath("Books.db"));
            // Отримання всіх книг з бази даних та встановлення їх в ListView
            List<Book> books = GetAllBooksFromDatabase();
            booksListView.ItemsSource = books;
            // Ініціалізація відтворювачів звуків
            successSoundPlayer.Open(new Uri("sound1.mp3", UriKind.Relative));
            deleteSoundPlayer.Open(new Uri("sound3.mp3", UriKind.Relative));
        }

        private void CreateNewDatabase()
        {
            try
            {
                // Використовуйте клас SQLiteConnection для підключення до бази даних
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    // Відкрийте з'єднання
                    connection.Open();

                    // Створіть команду для виконання SQL-запиту
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        // SQL-запит для створення таблиці (якщо вона не існує)
                        string createTableQuery = "CREATE TABLE IF NOT EXISTS Books (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT, Author TEXT, Genre TEXT, PageCount INTEGER)";

                        // Налаштуйте команду на виконання SQL-запиту
                        command.CommandText = createTableQuery;

                        // Виконайте SQL-запит
                        command.ExecuteNonQuery();
                    }
                }

                // Якщо немає помилок, виведіть повідомлення про успішне створення бази даних
                Console.WriteLine("База даних успішно створена.");
            }
            catch (Exception ex)
            {
                // Виведіть помилку, якщо сталася помилка при створенні бази даних
                Console.WriteLine("Помилка при створенні бази даних: " + ex.Message);
            }
        }

        // Отримання всіх книг з бази даних
        private List<Book> GetAllBooksFromDatabase()
        {
            List<Book> books = new List<Book>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM books";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Book book = new Book
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Title = Convert.ToString(reader["Title"]),
                                    Author = Convert.ToString(reader["Author"]),
                                    Genre = Convert.ToString(reader["Genre"]),
                                    PageCount = Convert.ToInt32(reader["PageCount"])
                                };
                                books.Add(book);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorWindow = new ErrorWindow("Помилка при зчитуванні книг з бази даних: " + ex.Message);
                errorWindow.ShowDialog();
            }

            return books;
        }

        // Обробник натискання кнопки "Додати книгу"
        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            // Отримання даних про нову книгу з текстових полів
            string title = titleTextBox.Text.Trim();
            string author = authorTextBox.Text.Trim();
            string pageCountText = pageCountTextBox.Text.Trim();

            // Перевірка правильності введених даних
            if (string.IsNullOrWhiteSpace(title))
            {
                var errorWindow = new ErrorWindow("Назва книги не була введена.");
                errorWindow.ShowDialog();
                return;
            }
            if (string.IsNullOrWhiteSpace(author))
            {
                var errorWindow = new ErrorWindow("Автор книги не був введений.");
                errorWindow.ShowDialog();
                return;
            }
            if (!int.TryParse(pageCountText, out int pageCount) || pageCount <= 0)
            {
                var errorWindow = new ErrorWindow("Кількість сторінок повинна бути цілим додатнім числом.");
                errorWindow.ShowDialog();
                return;
            }

            // Створення об'єкта нової книги
            Book newBook = new Book
            {
                Title = title,
                Author = author,
                PageCount = pageCount
            };

            // Додавання книги до бази даних
            AddBookToDatabase(newBook);

            // Очищення текстових полів
            titleTextBox.Text = string.Empty;
            authorTextBox.Text = string.Empty;
            pageCountTextBox.Text = string.Empty;

            // Оновлення списку книг
            List<Book> books = GetAllBooksFromDatabase();
            booksListView.ItemsSource = books;
        }

        // Обробник натискання кнопки "Відобразити весь список"
        private void ShowAllBooks_Click(object sender, RoutedEventArgs e)
        {
            // Отримання всіх книг з бази даних та їх відображення
            List<Book> books = GetAllBooksFromDatabase();
            booksListView.ItemsSource = books;
        }

        // Додавання книги до бази даних
        private void AddBookToDatabase(Book book)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO Books (Title, Author, Genre, PageCount) VALUES (@Title, @Author, @Genre, @PageCount)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Title", book.Title);
                        command.Parameters.AddWithValue("@Author", book.Author);
                        command.Parameters.AddWithValue("@Genre", book.Genre);
                        command.Parameters.AddWithValue("@PageCount", book.PageCount);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Відтворення звукового ефекту успішного додавання книги
                            successSoundPlayer.Position = TimeSpan.Zero;
                            successSoundPlayer.Play();
                            successLabel.Content = "Книга успішно додана!";
                            successLabel.Foreground = Brushes.Green;
                        }
                        else
                        {
                            var errorWindow = new ErrorWindow("Не вдалося додати книгу до бази даних.");
                            errorWindow.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorWindow = new ErrorWindow("Помилка при додаванні книги до бази даних: " + ex.Message);
                errorWindow.ShowDialog();
            }
        }

        // Обробник натискання кнопки "Пошук"
        private void SearchBooks_Click(object sender, RoutedEventArgs e)
        {
            string searchCriteria = searchTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(searchCriteria))
            {
                if (titleRadioButton.IsChecked == true)
                {
                    // Пошук книг за назвою
                    List<Book> foundBooks = SearchBooksByTitle(searchCriteria);
                    booksListView.ItemsSource = foundBooks;
                }
                else if (authorRadioButton.IsChecked == true)
                {
                    // Пошук книг за автором
                    List<Book> foundBooks = SearchBooksByAuthor(searchCriteria);
                    booksListView.ItemsSource = foundBooks;
                }
                else
                {
                    var errorWindow = new ErrorWindow("Виберіть критерій пошуку (за назвою або за автором).");
                    errorWindow.ShowDialog();
                }
            }
            else
            {
                var errorWindow = new ErrorWindow("Введіть критерій пошуку.");
                errorWindow.ShowDialog();
            }
        }

        // Обробник очищення текстового поля, якщо воно містить текст за замовчуванням
        private void ClearTextIfDefault(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag as string)
            {
                textBox.Text = string.Empty;
            }
        }

        // Пошук книг за назвою
        private List<Book> SearchBooksByTitle(string title)
        {
            List<Book> foundBooks = new List<Book>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM Books";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Book book = new Book
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Title = Convert.ToString(reader["Title"]),
                                    Author = Convert.ToString(reader["Author"]),
                                    Genre = Convert.ToString(reader["Genre"]),
                                    PageCount = Convert.ToInt32(reader["PageCount"])
                                };

                                // Додавання книги до результатів, якщо її назва містить введене слово
                                if (book.Title.IndexOf(title, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    foundBooks.Add(book);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorWindow = new ErrorWindow("Помилка при пошуку книг: " + ex.Message);
                errorWindow.ShowDialog();
            }

            return foundBooks;
        }

        // Пошук книг за автором
        private List<Book> SearchBooksByAuthor(string author)
        {
            List<Book> foundBooks = new List<Book>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM Books";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Book book = new Book
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Title = Convert.ToString(reader["Title"]),
                                    Author = Convert.ToString(reader["Author"]),
                                    Genre = Convert.ToString(reader["Genre"]),
                                    PageCount = Convert.ToInt32(reader["PageCount"])
                                };

                                // Додавання книги до результатів, якщо її автор містить введене ім'я
                                if (book.Author.IndexOf(author, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    foundBooks.Add(book);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorWindow = new ErrorWindow("Помилка при пошуку книг: " + ex.Message);
                errorWindow.ShowDialog();
            }

            return foundBooks;
        }

        // Обробник зміни виділення в ListView
        private void booksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Book selectedBook = booksListView.SelectedItem as Book;
            deleteBookButton.IsEnabled = selectedBook != null;
        }

        // Обробник натискання кнопки "Видалити книгу"
        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            // Отримання виділеної книги
            Book selectedBook = booksListView.SelectedItem as Book;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string query = "DELETE FROM Books WHERE Id = @Id";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", selectedBook.Id);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Відтворення звукового ефекту успішного видалення книги
                            deleteSoundPlayer.Position = TimeSpan.Zero;
                            deleteSoundPlayer.Play();
                            successLabel.Content = "Книга успішно видалена!";
                            successLabel.Foreground = Brushes.Goldenrod;
                        }
                        else
                        {
                            var errorWindow = new ErrorWindow("Не вдалося видалити книгу.");
                            errorWindow.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorWindow = new ErrorWindow("Помилка при видаленні книги: " + ex.Message);
                errorWindow.ShowDialog();
            }
            // Оновлення списку книг
            List<Book> books = GetAllBooksFromDatabase();
            booksListView.ItemsSource = books;
        }

        // Поля для збереження даних про сортування
        private GridViewColumnHeader lastHeaderClicked = null;
        private ListSortDirection lastDirection = ListSortDirection.Ascending;
        private double lastColumnWidth;

        // Обробник натискання заголовка стовпця GridViewColumn
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            // Отримання натисканого заголовка стовпця
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            string sortBy = headerClicked.Tag.ToString(); // Отримання назви стовпця, за яким буде проводитися сортування

            // Відновлення ширини попереднього стовпця
            if (lastHeaderClicked != null && lastHeaderClicked != headerClicked)
            {
                lastHeaderClicked.Column.Width = lastColumnWidth;
            }

            // Зміна напрямку сортування
            if (lastDirection == ListSortDirection.Ascending)
            {
                lastDirection = ListSortDirection.Descending;
            }
            else
            {
                lastDirection = ListSortDirection.Ascending;
            }

            lastHeaderClicked = headerClicked;
            lastColumnWidth = lastHeaderClicked.Column.Width;

            // Сортування даних
            Sort(sortBy, lastDirection);
        }

        // Метод для сортування даних
        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(booksListView.ItemsSource);
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }
}
