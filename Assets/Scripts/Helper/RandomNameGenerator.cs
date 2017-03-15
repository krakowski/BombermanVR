using System;

/// <summary>
///     The RandomNameGenerator class provides static methods for random name generation.
/// </summary>
public class RandomNameGenerator {

    //================================================================================
    // Private Properties
    //================================================================================

    // Colors (comment to prevent "unused"-warning)
    // private static string[] colors = new string[] { "Black", "Blue", "Green", "Gold", "Grey", "Orange", "Purple", "Red", "Silver", "White" };

    // Animals
    private static string[] animals = new string[] { "Ape", "Alligator", "Ant", "Bat", "Bear", "Bird", "Butterfly", "Cat", "Chameleon", "Chicken", "Crab", "Dog", "Dolphin", "Donkey", "Duck", "Eagle", "Elephant", "Falcon", "Fish", "Fox", "Frog", "Gecko", "Goat", "Gorilla", "Hamster", "Horse", "Jackal", "Jaguar", "Kangaroo", "Koala", "Lemming", "Leopard", "Lion", "Lizard", "Lobster", "Mole", "Mouse", "Octopus", "Otter", "Parrot", "Penguin", "Pig", "Piranha", "Rabbit", "Raccoon", "Rat", "Scorpion", "Seal", "Shrimp", "Snail", "Snake", "Tiger", "Tortoise", "Walrus", "Weasel", "Wolf", "Wombat", "Zebra" };

    // Adjectives
    private static string[] adjectives = new string[] { "Adorable", "Acrobatic", "Afraid", "Amazing", "Angry", "Artistic", "Athletic", "Attractive", "Awesome", "Beautiful", "Blind", "Brave", "Brilliant", "Calm", "Cheerful", "Clever", "Cold", "Confused", "Cool", "Crazy", "Creepy", "Criminal", "Cute", "Evil", "Exotic", "Fabulous", "Fake", "Fat", "Funny", "Giant", "Glamorous", "Glorious", "Great", "Hairy", "Happy", "Intelligent", "Lame", "Lazy", "Mad", "Monstrous", "Mysterious", "Optimistic", "Polite", "Poor", "Pretty", "Rich", "Royal", "Sad", "Serious", "Sleepy", "Small", "Smart", "Spectacular", "Terrific" };

    // Random Number provider
    private static Random rand = new Random();

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Returns a random Player name.
    /// </summary>
    /// <returns>Random player name</returns>
    public static string generatePlayerName() {
        int animalIndex = rand.Next(0, animals.Length);
        string animal = animals[animalIndex];

        int adjectiveIndex = rand.Next(0, adjectives.Length);
        string adjective = adjectives[adjectiveIndex];

        return adjective + animal;
    }
    
    /// <summary>
    ///     Returns a random Room name.
    /// </summary>
    /// <returns>Random room name</returns>
    public static string generateRoomName() {
        int animalIndex = rand.Next(0, animals.Length);
        string animal = animals[animalIndex];

        int adjectiveIndex = rand.Next(0, adjectives.Length);
        string adjective = adjectives[adjectiveIndex];

        return adjective + ' ' + animal;
    }
}
