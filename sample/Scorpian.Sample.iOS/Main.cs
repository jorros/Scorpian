using Scorpian;
using Scorpian.iOS_test;
using Scorpian.Sample;

//using Scorpian.iOS_test;

//UIApplication.Main (args, null, typeof (AppDelegate));

var settings = new EngineSettings
{
    Name = "Scorpian Sample",
    DisplayName = "Scorpian Sample"
};
var game = new SampleGame();

game.Run(settings, args);