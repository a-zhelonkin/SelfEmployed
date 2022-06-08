# Инспектор самозанятых

<img src="https://github.com/a-zhelonkin/SelfEmployed/blob/master/App/appicon.ico"/>

Инструмент для проверки самозанятости ФЛ по ИНН. Как пользоваться:
- Скачать [SelfEmployed.App.exe](https://github.com/a-zhelonkin/SelfEmployed/releases/latest/download/Release-win-x64.zip) из [последнего релиза](https://github.com/a-zhelonkin/SelfEmployed/releases/latest)
- Положить рядом с файлом `inns.txt`
- Каждый ИНН должен быть на новый строке без разделителей
- Запустить `SelfEmployed.App.exe`

В процессе проверки будут созданы файлы:
- `inns.self-employed.txt` – список ИНН самозанятых
- `inns.common-person.txt` – список ИНН не самозанятых

После перезапуска программа попробует вычитать список успешно проверенных ИНН из соответствующих файлов, чтобы не проверять их снова
