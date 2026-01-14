namespace DataMaker
{
    /// <summary>
    /// Generates random address data.
    /// </summary>
    public static class AddressDataGenerator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generates a random address.
        /// </summary>
        /// <returns>An <see cref="AddressData"/> object with a random street, city, and state.</returns>
        public static AddressData Generate()
        {
            var street = $"{_random.Next(9999)} {StreetNames[_random.Next(StreetNames.Count)]} {StreetSuffixes[_random.Next(StreetSuffixes.Count)]}";
            var city = CityNames[_random.Next(CityNames.Count)];
            var state = UsStates[_random.Next(UsStates.Count)];
            return new AddressData(street, city, state);
        }

        private static readonly List<string> UsStates = new List<string>
        {
            "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia",
            "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland",
            "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey",
            "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina",
            "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"
        };

        private static readonly List<string> CityNames = new List<string>
        {
            "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "Jacksonville",
            "Fort Worth", "San Jose", "Austin", "Charlotte", "Columbus", "Indianapolis", "San Francisco", "Seattle", "Denver", "Washington",
            "Boston", "El Paso", "Nashville", "Detroit", "Oklahoma City", "Las Vegas", "Portland", "Memphis", "Louisville", "Milwaukee",
            "Baltimore", "Albuquerque", "Tucson", "Fresno", "Sacramento", "Mesa", "Kansas City", "Atlanta", "Colorado Springs", "Raleigh",
            "Omaha", "Miami", "Long Beach", "Virginia Beach", "Oakland", "Minneapolis", "Tulsa", "Arlington", "New Orleans", "Wichita",
            "Cleveland", "Tampa", "Aurora", "Santa Ana", "Anaheim", "St. Louis", "Riverside", "Corpus Christi", "Lexington", "Pittsburgh",
            "Anchorage", "Stockton", "Cincinnati", "St. Paul", "Toledo", "Greensboro", "Newark", "Plano", "Henderson", "Lincoln",
            "Orlando", "Jersey City", "Chula Vista", "Fort Wayne", "Chandler", "St. Petersburg", "Laredo", "Scottsdale", "North Las Vegas", "Madison",
            "Lubbock", "Reno", "Garland", "Hialeah", "Akron", "Rochester", "Irving", "Chesapeake", "Glendale", "Winston-Salem",
            "Boise", "Norfolk", "Frisco", "Paradise", "San Bernardino", "Spokane", "Fayetteville", "Tacoma", "Oxnard", "Fontana",
            "Moreno Valley", "Augusta", "Little Rock", "Amarillo", "Mobile", "Grand Rapids", "Huntsville", "Montgomery", "Des Moines", "Richmond",
            "Baton Rouge", "Newport News", "Cape Coral", "Providence", "Chattanooga", "Tempe", "Springfield", "Vancouver", "Fort Lauderdale", "Salinas",
            "Santa Clarita", "Oceanside", "Garden Grove", "Rancho Cucamonga", "Santa Rosa", "Port St. Lucie", "Peoria", "Denton", "Pasadena", "Killeen",
            "Syracuse", "Sunnyvale", "Gainesville", "Alexandria", "Elizabeth", "Waco", "Palmdale", "Hayward", "Pomona", "Escondido",
            "Joliet", "Naperville", "Hollywood", "Clarksville", "Torrance", "Paterson", "Savannah", "Bridgeport", "Metairie", "Roseville",
            "Sterling Heights", "Miramar", "Coral Springs", "Carrollton", "Fullerton", "Hampton", "Pompano Beach", "Cedar Rapids", "Eugene", "Grand Prairie",
            "Olathe", "Elk Grove", "Corona", "Salem", "McKinney", "Lancaster", "Cary", "Stamford", "Warren", "West Valley City",
            "Victorville", "Modesto", "Huntington Beach", "Yonkers", "Charleston", "Tallahassee", "Knoxville", "Provo", "Brownsville", "New Haven",
            "Fargo", "Sioux Falls", "Erie"
        };

        private static readonly List<string> StreetSuffixes = new List<string>
        {
            "St", "Ave", "Blvd", "Rd", "Ln", "Dr", "Ct", "Pl", "Terrace", "Way"
        };

        private static readonly List<string> StreetNames = new List<string>
        {
            "Main", "Second", "Third", "First", "Oak", "Fourth", "Elm", "Pine", "Church", "Maple",
            "Walnut", "Fifth", "Washington", "Sixth", "Center", "High", "Park", "Cedar", "North", "Seventh",
            "Sunset", "River", "South", "Chestnut", "Ridge", "Mill", "Cherry", "Lakeview", "Spring", "Railroad",
            "Meadow", "School", "Circle", "West", "Eighth", "Spruce", "Jackson", "Shady", "Jefferson", "Hill",
            "Lincoln", "Short", "Water", "Ninth", "Locust", "View", "Lake", "Tenth", "Eleventh", "Twelfth",
            "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth", "Nineteenth", "Twentieth", "Broadway", "Grand",
            "Central", "Ocean", "Beach", "Forest", "Valley", "Summit", "Creek", "Stone", "Green", "Garden",
            "Canal", "Bridge", "Cross", "Front", "Back", "East", "King", "Queen", "Prince", "Princess",
            "Royal", "Imperial", "Colonial", "Heritage", "Freedom", "Liberty", "Independence", "Constitution", "Patriot", "Union",
            "Federal", "State", "County", "City", "Town Line", "Village", "Homestead", "Farm", "Orchard", "Vineyard",
            "Harvest", "Country Club", "Golf Course", "Sportsman", "Hunter", "Fisherman's", "Pioneer", "Explorer", "Traveler's", "Pilgrim",
            "Settler's", "Old Mill", "Old Post", "Old Stage", "Old Farm", "Old Town", "New Town", "Modern", "Future", "Innovation",
            "Technology", "Science", "University", "College", "Campus", "Library", "Museum", "Art Gallery", "Theater", "Music Hall",
            "Concert", "Festival", "Celebration", "Jubilee", "Victory", "Triumph", "Honor", "Glory", "Memorial", "Veteran's",
            "Soldier's", "Sailor's", "Airman's", "Marine", "Coast Guard", "Police", "Fire Station", "Hospital", "Clinic", "Doctor's",
            "Nurse's", "Pharmacy", "Health Center", "Wellness", "Fitness", "Recreation", "Playground", "Sports Complex", "Stadium", "Arena",
            "Convention Center", "Exhibition", "Fairgrounds", "Market", "Commerce", "Industry", "Business Park", "Corporate", "Executive", "Enterprise",
            "Venture", "Prosperity", "Fortune", "Wealth", "Success", "Achievement", "Progress", "Development", "Growth", "Expansion",
            "Horizon", "Vista", "Panorama", "Scenic", "Parkway", "Boulevard", "Esplanade", "Promenade", "Boardwalk", "Causeway",
            "Turnpike", "Freeway", "Expressway", "Highway", "Interstate", "Route", "Trail", "Path", "Lane", "Road",
            "Drive", "Way", "Court", "Square", "Plaza", "Place", "Row", "Terrace", "Gardens", "Estates",
            "Manor", "Heights", "Hills", "Woods", "Grove", "Field", "Prairie", "Savannah", "Canyon", "Bluff",
            "Cliff", "Cove", "Bay", "Harbor", "Port", "Wharf", "Marina", "Shore", "Island", "Peninsula",
            "Delta", "Oasis", "Desert", "Mountain", "Peak", "Glen", "Dale", "Hollow", "Brook", "Pond",
            "Falls", "Cascade", "Well", "Fountain", "Reservoir", "Dam", "Lock", "Tunnel", "Underpass", "Overpass",
            "Viaduct", "Aqueduct", "Embankment", "Levee", "Dike", "Wall", "Gate", "Door", "Window", "Roof",
            "Floor", "Ceiling", "Room", "Hall", "Chamber", "Gallery", "Arcade", "Colonnade", "Portico", "Balcony",
            "Patio", "Deck", "Porch", "Veranda", "Gazebo", "Pergola", "Arbor", "Trellis", "Hedge", "Fence",
            "Post", "Pillar", "Column", "Statue", "Monument", "Obelisk", "Arch", "Tower", "Spire", "Dome",
            "Cupola", "Steeple", "Bell Tower", "Clock Tower", "Lighthouse", "Windmill", "Water Tower", "Silo", "Barn", "Stable",
            "Corral", "Paddock", "Pasture", "Range", "Ranch", "Plantation", "Villa", "Cottage", "Cabin", "Lodge",
            "Chalet", "Bungalow", "Townhouse", "Apartment", "Condo", "Loft", "Studio", "Penthouse", "Duplex", "Triplex",
            "Quadplex", "Rowhouse", "Brownstone", "Greystone", "Bluestone", "Sandstone", "Limestone", "Granite", "Marble", "Crystal",
            "Diamond", "Gold", "Silver", "Bronze", "Copper", "Iron", "Steel", "Aluminum", "Titanium", "Platinum",
            "Emerald", "Ruby", "Sapphire", "Pearl", "Coral", "Shell", "Sand", "Rock", "Pebble", "Boulder",
            "Crest", "Top", "Bottom", "Side", "Edge", "Corner"
        };
    }
}
