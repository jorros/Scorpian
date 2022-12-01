using Scorpian;
using Scorpian.Sample;

var settings = new EngineSettings
{
    Name = "Scorpian Sample",
    DisplayName = "Scorpian Sample"
};
var game = new SampleGame();

game.Run(settings);