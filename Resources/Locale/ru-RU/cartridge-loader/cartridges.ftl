device-pda-slot-component-slot-name-cartridge = Картридж
default-program-name = Программа
notekeeper-program-name = Заметки
nano-task-program-name = НаноЗадачи
news-read-program-name = Новости станции
crew-manifest-program-name = Манифест экипажа
crew-manifest-cartridge-loading = Загрузка...
net-probe-program-name = Зонд сетей
net-probe-scan = Просканирован { $device }!
net-probe-label-name = Название
net-probe-label-address = Адрес
net-probe-label-frequency = Частота
net-probe-label-network = Сеть
log-probe-program-name = Зонд логов
log-probe-scan = Загружены логи устройства { $device }!
log-probe-label-time = Время
log-probe-label-accessor = Использовано:
log-probe-label-number = #
log-probe-print-button = Распечатать
log-probe-printout-device = Отсканированное устройство: { $name }
log-probe-printout-header = Последние логи:
log-probe-printout-entry = #{ $number } / { $time } / { $accessor }
astro-nav-program-name = АстроНав
med-tek-program-name = МедТек
# Wanted list cartridge
wanted-list-program-name = Список разыскиваемых
nano-task-ui-heading-high-priority-tasks =
    { $amount ->
        [zero] Нету высоко приоритетных задач
        [one] 1 высоко приоритетная задача
       *[other] { $amount } высоко приоритетных задач
    }
nano-task-ui-heading-medium-priority-tasks =
    { $amount ->
        [zero] Нету приоритетных задач
        [one] 1 приоритетная задача
       *[other] { $amount } приоритетных задач
    }
nano-task-ui-heading-low-priority-tasks =
    { $amount ->
        [zero] Нету низко приоритетных задач
        [one] 1 низко приоритетная задача
       *[other] { $amount } низко приоритетных задач
    }
nano-task-ui-done = Готово
nano-task-ui-revert-done = Отменить
nano-task-ui-priority-low = Низкий
nano-task-ui-priority-medium = Средний
nano-task-ui-priority-high = Высокий
nano-task-ui-cancel = Отмена
nano-task-ui-print = Печать
nano-task-ui-delete = Удалить
nano-task-ui-save = Сохранить
nano-task-ui-new-task = Новая задача
nano-task-ui-description-label = Описание:
nano-task-ui-description-placeholder = Получить что-то важное
nano-task-ui-requester-label = Заявитель:
nano-task-ui-requester-placeholder = Джон NanoTrasen
nano-task-ui-item-title = Редактировать задачу
nano-task-printed-description = Описание: { $description }
nano-task-printed-requester = Заявитель: { $requester }
nano-task-printed-high-priority = Приоритет: Высокий
nano-task-printed-medium-priority = Приоритет: Средний
nano-task-printed-low-priority = Приоритет: Низкий
wanted-list-label-no-records = Все спокойно, ковбой.
wanted-list-search-placeholder = Поиск по имени и статусу
wanted-list-age-label = [color=darkgray]Возраст:[/color] [color=white]{ $age }[/color]
wanted-list-job-label = [color=darkgray]Должность:[/color] [color=white]{ $job }[/color]
wanted-list-species-label = [color=darkgray]Вид:[/color] [color=white]{ $species }[/color]
wanted-list-gender-label = [color=darkgray]Гендер:[/color] [color=white]{ $gender }[/color]
wanted-list-reason-label = [color=darkgray]Причина:[/color] [color=white]{ $reason }[/color]
wanted-list-unknown-reason-label = неизвестная причина
wanted-list-initiator-label = [color=darkgray]Инициатор:[/color] [color=white]{ $initiator }[/color]
wanted-list-unknown-initiator-label = неизвестный инициатор
wanted-list-status-label = [color=darkgray]статус:[/color] { $status ->
        [suspected] [color=yellow]подозревается[/color]
        [wanted] [color=red]разыскивается[/color]
        [detained] [color=#b18644]под арестом[/color]
        [paroled] [color=green]освобождён по УДО[/color]
        [discharged] [color=green]освобождён[/color]
       *[other] нет
    }
wanted-list-history-table-time-col = Время
wanted-list-history-table-reason-col = Преступление
wanted-list-history-table-initiator-col = Инициатор
