# Конвенции

1. Протокол стремится быть похожим на http, вырезая лишнее и исправляя глупости

2. Фреймворк стремится быть похожим внешне на asp.net core

3. Builder-style и controller-style равноправны.  
Функционал обязан быть представлен либо в обоих стилях, либо нигде.

# Ubiquitous language
## Package - пакет
Упакованные данные для одной отправки по udp.  

Отвергнутые синонимы: message, paket, body, letter, query, event

# Peer
Субъект, способный передавать и принимать пакеты

# Connection
Объединение пиров в общую сеть

Отвергнутые синонимы: session, network, room, guild

# Server
Главный пир в сети. Координирует коннекшн. Также является полноправным отправителем и получателем пакетов.

# Client
Рядовой пир в сети. Подсоединяется к существующему коннекшну, отправляет и получает пакеты.

## Endpoint
Сетевой публичный интерфейс фреймворка, делегирующий вызов на определенный метод ресивера.

Отвергнутые синонимы: method, route, topic, queue, address

### Configuration endpoint
Эндпоинт, настройки соединения. Не является смысловым, не вызывает бизнес-логику. 
Первый сегмент роута всегда `fat-net-lib`.

Отвергнутые синонимы: connection endpoint, initial endpoint

### Initial endpoint
Специфичный конфигурационный эндпоинт. Вызывается только клиентом на сервере и только сразу после старта специальным раннером.

Отвергнутые синонимы: initial configuration endpoint

## Sender
Отправитель пакета в контексте определенного эндпоинта.  

Отвергнутые синонимы: client, transmitter, publisher, producer

## Receiver
Получатель пакета в контексте определенного эндпоинта. 

Отвергнутые синонимы: server, subscriber, consumer

## Exchange - обмен
Отправка пары связанных пакетов в режиме запрос-ответ.

Отвергнутые синонимы: transaction, interchange, discourse, handshake, request-response, question-and-answer, query-and-answer, query  call-and-response, event, session, sequence, conversation, communication, operation, interaction, message, round-trip, transceival, connection

## Route
Идентификатор эндпоинта.  

Отвергнутые синонимы: url, uri, path, request-mapping

## Route segment
Часть роута, отделенная символами `/`.  

Отвергнутые синонимы: route, part,

## Action
Последний роут сегмент, описывающий характер действия эндпоинта.  

Отвергнутые синонимы: verb

## Controller-style api
Стиль маршрутизации, похожий на asp.net core или spring mvc, где каждый эндпоинт выражается декларацией метода класса controller с атрибутами/аннотациями. 

Отвергнутые синонимы: controller-like, old-controller-style

## Builder-style api
Стиль маршрутизации, похожий на asp.net core или spark-java, где каждый эндпоинт описывается в объекте, реализующем билдер-паттерн.  

Отвергнутые синонимы: builder-like, new-controller-style
