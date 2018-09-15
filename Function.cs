using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace AWSLambda3
{
    public class Function
    {
        //added classes in order to serialize and deserialize object easely and keep code more cleen and obvious
        public class PickUpLocation
        {
            public PickUpLocation(float latitude, float longitude)
            {
                Latitude = latitude;
                Longitude = longitude;
            }

            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }

        public class Unicorn
        {
            public Unicorn(string name, string color, string gender)
            {
                this.Name = name;
                this.Color = color;
                this.Gender = gender;
            }

            public string Name { get; set; }
            public string Color { get; set; }
            public string Gender { get; set; }
        }

        public class RideResponse
        {
            public RideResponse(string orderId, Unicorn unicorn, string unicornName, string eta, string rider)
            {
                OrderId = orderId;
                Unicorn = unicorn;
                UnicornName = unicornName;
                Eta = eta;
                Rider = rider;
            }

            public string OrderId { get; set; }
            public Unicorn Unicorn { get; set; }
            public string UnicornName { get; set; }
            public string Eta { get; set; }
            public string Rider { get; set; }
            public List<WeatherForcast> Weather { get; set; }
        }

        public class WeatherForcast
        {
            public WeatherForcast(int id, string main, string description, string icon)
            {
                this.id = id;
                this.main = main;
                this.description = description;
                this.icon = icon;
            }

            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

        //pick-up-location we will get from body
        PickUpLocation body;

        //dynamoDB new client in order to write to Orders table later
        AmazonDynamoDBClient ddb = new AmazonDynamoDBClient();

        //some hardcoded fleet for now
        Unicorn[] fleet = { new Unicorn("Bucephalus", "Golden", "Male"), new Unicorn("Shadowfax", "White", "Male"), new Unicorn("Rocinante", "Yellow", "Female") };

        //Lambda entery point - ****maybe i can with the handler serializer get the pickup location in constructor...
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            //returning a bad 'response' if something not went wall
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };

            try
            {
                //inserting a byte array in order to return a random string orderId
                string orderId = toUrlString(new byte[32]);

                //requester user name from cognito user pool
                string userName = request.RequestContext.Authorizer.Claims.GetValueOrDefault("cognito:username");

                //taking the body of the apigateway lambda request
                var deserialize = JObject.Parse(request.Body);
                this.body = JsonConvert.DeserializeObject<PickUpLocation>(deserialize["PickupLocation"].ToString());
                

                //logging the event we recieved
                LambdaLogger.Log("Received event (" + orderId + "): " + "Latitude: " + body.Latitude + ",Longitude: " + body.Longitude + "...");

                //getting the unicorn nearest to the desired location (for now returning just a random one)
                PickUpLocation pickUpLocation = this.body;
                Unicorn unicorn = findUnicorn(pickUpLocation);

                //adding the ride to dynamoDB
                await recordRide(orderId, userName, unicorn);

                //prepering the response of our apigateway
                RideResponse ride = new RideResponse(orderId, unicorn, unicorn.Name, "30 seconds", userName);

                //adding and logging the weather from a weather api service call for the given lat and lon
                ride.Weather = await GetWeatherForecast(pickUpLocation.Latitude,pickUpLocation.Longitude);

                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(ride),
                    Headers = new Dictionary<string, string> { { "Access-Control-Allow-Origin", "*" } }
                };

            }
            catch (Exception e)
            {
                context.Logger.LogLine("Error in inserting a new item to the database and/or returning a valid value");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
            }

            return response;
        }

        //recording the ride at dynamoDB
        public async Task recordRide(string orderId, string userName, Unicorn unicorn)
        {
            //loading the designated table for the ride - dynamodb document model
            Table orders = Table.LoadTable(ddb, "Orders");

            //can use in attributeValue in here also
            Document order = new Document();
            order["OrderId"] = orderId;
            order["User"] = userName;
            order["Unicorn"] = JsonConvert.SerializeObject(unicorn);
            order["UnicornName"] = unicorn.Name;
            order["RequestTime"] = DateTime.Now.ToString();
            //putting items in the table
            await orders.PutItemAsync(order);

            ////attributeValue method
            //var client = new AmazonDynamoDBClient();
            //var request = new PutItemRequest
            //{
            //    TableName = "Orders",
            //    Item = new Dictionary<string, AttributeValue> {
            //        { "OrderId", new AttributeValue { S = orderId } },
            //        { "User", new AttributeValue { S = userName } },
            //        { "Unicorn", new AttributeValue { S = JsonConvert.SerializeObject(unicorn)} },
            //        { "UnicornName", new AttributeValue { S = unicorn.name } },
            //        { "RequestTime", new AttributeValue { S = DateTime.Now.ToString() } }
            //    }
            //};

            //await client.PutItemAsync(request);
        }

        //returning a random unicorn for now
        public Unicorn findUnicorn(PickUpLocation pickupLocation)
        {
            LambdaLogger.Log("Finding unicorn for ," + pickupLocation.Latitude + ", " + pickupLocation.Longitude);
            //returning a random horse from the fleet, in real life we need a better logic of course.
            return fleet[new Random().Next(3)];
        }

        //returning a random id from a random byte array for the orderId
        public string toUrlString(byte[] buffer)
        {
            new Random().NextBytes(buffer);
            return Convert.ToBase64String(buffer, 0, 16);
        }

        //get the weather from the web service openWeatherMap
        public async Task<List<WeatherForcast>> GetWeatherForecast(double lat, double lon)
        {
            HttpClient httpn = new HttpClient();
            string uri = String.Format("http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&appid=89e2d42e2a2a45e194aa17b364a464c7", lat, lon);
            HttpResponseMessage response = await httpn.GetAsync(uri);
            string result = await response.Content.ReadAsStringAsync();
            var deserialize = JObject.Parse(result);
            List<WeatherForcast> weather = JsonConvert.DeserializeObject<List<WeatherForcast>>(deserialize["weather"].ToString());
            LambdaLogger.Log("The weather at the target location is: " + weather[0].main + " that means actually: " + weather[0].description);
            return weather;
        }
    }
}
