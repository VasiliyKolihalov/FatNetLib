# Получение данных из пакета.
Эндпоинты в контроллере могут получать данные из пакета.

Для получение данных есть несколько опций:
1. Неявное получение 
    ```c#
    public void AddItem(Item item, Auth auth) { }
    ```
    Фреймворк сам десериализует данные из пакета, если название поля в пакете совпадает с название типа данных в аргументах эндпоинта.


2. Явное получение
    ```c#
    public void AddItem([FromBody] Item item, [FromAuth] Auth auth) { }
    ```
    При желании, можно явно указать откуда десериализововать данные.
    Для стандартных полей пакета есть атрибуты `[FromBody]`, `[FromAuth]` и т.д.

    Для нестрандартных полей есть атрибут `[FromPackage]`. Пример использования:
    ```c#
    public void AddItem([FromPackage("ItemData")] ItemData itemData) { }
    ```
    `[FromPackage]` также можно использовать для получения стандартных полей и для получения всего пакета:
    ```c#
    public void AddItem([FromPackage("Body")] Item item) { }
    ```
    ```c#
    public void AddItem([FromPackage] Package package) { }
    ```



