# SelfEmployed

Инструмент для проверки самозанятости ФЛ по ИНН. Как пользоваться:
- Скачать [SelfEmployed.App.exe](https://github.com/a-zhelonkin/SelfEmployed/releases/latest/download/SelfEmployed.App.exe) из [последнего релиза](https://github.com/a-zhelonkin/SelfEmployed/releases/latest)
- Положить рядом с файлом `inns.txt`
- Каждый ИНН должен быть на новый строке без разделителей
- Запустить `SelfEmployed.App.exe`

В процессе проверки будут созданы файлы:
- `inns.self-employed.txt` – список ИНН самозанятых
- `inns.common-person.txt` – список ИНН не самозанятых
- `inns.poor-response.txt` – список ИНН с ошибками
