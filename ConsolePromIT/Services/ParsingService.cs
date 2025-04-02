using ConsolePromIT.Core.Models;
using ConsolePromIT.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsolePromIT.Services
{
    public class ParsingService
    {

        private readonly Regex _wordRegex = new Regex(@"\b[a-zA-Zа-яА-ЯёЁ]{4,20}\b", RegexOptions.Compiled);
        private readonly DataContext _dataContext;        
        private readonly ILogger<ParsingService> _logger;
        private const int MIN_CONST_WORD = 3;

        public ParsingService(DataContext dataContext , ILogger<ParsingService> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public void ParsingFile()
        {
            while (true)
            {

                _logger.LogWarning("Введите путь к файлу (или 'exit' для выхода):");
                var filePath = Console.ReadLine();

                #region Validation

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogError("Путь к файлу не указан");
                    continue;
                }

                if (filePath.Equals("exit"))
                {
                    break;
                }

                if (!File.Exists(filePath))
                {
                    _logger.LogError("Файл не существует");
                    continue;
                }

                if (!IsUtf8(filePath))
                {
                    _logger.LogError("Файл НЕ в UTF-8.");
                    continue;
                }

                if (Path.GetExtension(filePath).ToLower() != ".txt")
                {
                    _logger.LogError("Ошибка: разрешены только файлы с расширением .txt");
                    continue;
                }
                #endregion

                try
                {        
                    var wordStatistics = new List<WordStatistic>();

                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            var matches = _wordRegex.Matches(line)
                                .Cast<Match>()
                                .Select(m => m.Value.ToLower().Replace('ё', 'е'));

                            foreach (var word in matches)
                            {
                                var existingStat = wordStatistics.FirstOrDefault(x => x.Word.Equals(word));
                                if (existingStat != null)
                                {
                                    existingStat.Count++;
                                }
                                else
                                {
                                    wordStatistics.Add(new WordStatistic { Word = word });
                                }
                            }
                        }
                    }

                    var existingWords = _dataContext.WordStatistics
                                                     .Where(x => wordStatistics.Select(ws => ws.Word).Contains(x.Word))
                                                     .ToList();

                    var wordsToAdd = new List<WordStatistic>();

                    foreach (var stat in wordStatistics)
                    {
                        var dbWord = existingWords.FirstOrDefault(x => x.Word == stat.Word);
                        if (dbWord is not null)
                        {
                            dbWord.Count += stat.Count;
                        }
                        else if (stat.Count >= MIN_CONST_WORD)
                        {
                            wordsToAdd.Add(stat);
                        }
                    }

                    _dataContext.WordStatistics.AddRange(wordsToAdd);
                    _dataContext.SaveChanges();

                    _logger.LogInformation($"Обработано {wordStatistics.Count} уникальных слов");
                    _logger.LogInformation($"Добавлено {wordsToAdd.Count} новых слов");
                    _logger.LogInformation($"Обновлено {existingWords.Count} существующих слов");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка: {ex.Message}");
                }
            }
        }

        public bool IsUtf8(string filePath)
        {
            byte[] buffer = File.ReadAllBytes(filePath);

            try
            {
                Encoding utf8 = new UTF8Encoding(false, true);
                utf8.GetString(buffer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
