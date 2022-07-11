# Конвенции

1. Поддержка как layered architecture, так и ports-and-adapters architecture.  
Хоть layered architecture и является сверх-популярной и простой архитектурой, ports-and-adapters является более корректной и продуманной. 

2. Протокол стремится быть похожим на http, вырезая лишнее и исправляя глупости

3. Фреймворк стремится быть похожим внешне на .net framework

4. Builder-style и controller-style равноправны.  
Функционал обязан быть представлен либо в обоих стилях, либо нигде.

# Ubiquitous language
## Package - пакет
Упакованные данные для одной отправки по udp.  

Отвергнутые синонимы: message, paket, body, letter, query, event

## Endpoint
Сетевой публичный интерфейс фреймворка, делегирующий вызов на определенный метод ресивера.

Отвергнутые синонимы: method, route, topic, queue, address

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
Стиль маршрутизации, похожий на .net framework или spring mvc, где каждый эндпоинт выражается декларацией метода класса controller с атрибутами/аннотациями. 

Отвергнутые синонимы: controller-like, old-controller-style

## Builder-style api
Стиль маршрутизации, похожий на .net core или spark-java, где каждый эндпоинт описывается в объекте, реализующем билдер-паттерн.  

Отвергнутые синонимы: builder-like, new-controller-style
