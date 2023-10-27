## Package

A package is a set of data to be sent once from sender to receiver.
Rejected synonyms: message, paket, body, letter, query, event

## Peer

A subject capable of sending and receiving packages.

## Connection

Joining peers into a common network.
Rejected synonyms: session, network, room, guild

## Server

The main peer in the network. Coordinates the connection. It also performs a peer role.

## Client

An ordinary peer of the network. Joins to an existing connection, sends and receives packagess.

## Endpoint

Delivery interface designed for transferring packages over the network or locally
Rejected synonyms: method, route, topic, queue, address

## Initializer

Network endpoint for initial connection setup.
Rejected synonyms: initial endpoint, connection endpoint, initial configuration endpoint

## Sender

The shipping peer in the context of a specific endpoint.
Rejected synonyms: client, transmitter, publisher, producer

## Receiver

The recipient peer in the context of a specific endpoint.
Rejected synonyms: server, subscriber, consumer

## Exchange

Sending a pair of related packages in request-response mode.

Rejected synonyms: transaction, interchange, discourse, handshake, request-response, question-and-answer,
query-and-answer, query call-and-response, event, session, sequence, conversation, communication, operation,
interaction, message, round-trip, connection

## Route

The endpoint identifier.
Rejected synonyms: url, uri, path, request-mapping

## Route segment

The part of the route separated by the `/` characters.
Rejected synonyms: route, part,

## Controller-style api

Routing style inspired by ASP.NET Core or Spring MVC controllers.

Rejected synonyms: controller-like, old-controller-style

## Builder-style api

Routing style inspired by ASP.NET Core or Minimal API and Spark Java endpoints.
Rejected synonyms: builder-like, new-controller-style
