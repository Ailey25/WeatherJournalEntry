# Weather Journal Backend

Weather journal allows the creation, management, and viewing of diary entries.
Weather information is queried from Weather API by the backend
City ID can be obtained by downloading the `city.list.json.gz` file from [OpenWeatherMap](http://bulk.openweathermap.org/sample/)

This is the backend portion of Weather Journal, and it contains Weather and User data.

`C#` `.NET Core` `Entity Framework`

###### Architecture
- Controller -> Service -> Unit Of Work -> Repository -> Database
