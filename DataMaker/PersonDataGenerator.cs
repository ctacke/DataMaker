namespace DataMaker
{
    /// <summary>
    /// Generates random person data, including first and last names.
    /// </summary>
    public static class PersonDataGenerator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generates a random person with a first and last name.
        /// </summary>
        /// <returns>A <see cref="PersonData"/> object with a random first and last name.</returns>
        public static PersonData Generate()
        {
            return new PersonData(
                GetRandomFirstName(),
                GetRandomLastName()
            );
        }

        private static string GetRandomFirstName()
        {
            return FirstNames[_random.Next(FirstNames.Count)];
        }

        private static string GetRandomLastName()
        {
            return LastNames[_random.Next(LastNames.Count)];
        }

        private static readonly List<string> FirstNames = new List<string>
        {
            "Aaliyah", "Aaron", "Abigail", "Adam", "Adeline", "Adrian", "Aisha", "Aiko", "Akira", "Alan",
            "Albert", "Alejandro", "Alessandra", "Alex", "Alexander", "Alice", "Amanda", "Amber", "Amir", "Amara",
            "Ana", "Ananya", "Andrea", "Andres", "Angela", "Ann", "Anna", "Anthony", "Anya", "Arjun",
            "Arthur", "Ashley", "Astrid", "Aurora", "Austin", "Ayana", "Barbara", "Ben", "Benjamin", "Betty",
            "Beverly", "Bianca", "Billy", "Bobby", "Bodhi", "Brandon", "Brenda", "Brian", "Brittany", "Bruce",
            "Bryan", "Caleb", "Camila", "Carl", "Carlos", "Carmen", "Carol", "Carolyn", "Catalina", "Catherine",
            "Cathy", "Charles", "Charlotte", "Cheryl", "Chris", "Christian", "Christina", "Christine", "Ciro", "Clara",
            "Cynthia", "Daniel", "Danielle", "David", "Debra", "Deborah", "Delilah", "Denise", "Dennis", "Diana",
            "Diego", "Dylan", "Donald", "Donna", "Doris", "Dorothy", "Douglas", "Edward", "Eleanor", "Elena",
            "Elijah", "Elizabeth", "Emilia", "Emily", "Emma", "Enya", "Eric", "Ethan", "Eugene", "Eva",
            "Evelyn", "Fatima", "Felix", "Fiona", "Frances", "Frank", "Gabriel", "Gabriela", "Gabrielle", "Gael",
            "Gary", "Genesis", "George", "Gerald", "Giovanni", "Giselle", "Gloria", "Grace", "Gregory", "Hannah",
            "Harold", "Harry", "Haruto", "Hazel", "Heather", "Hector", "Helen", "Helena", "Henry", "Hiroshi",
            "Howard", "Imani", "Isaac", "Isabella", "Isaiah", "Ivan", "Jack", "Jacqueline", "Jacob", "James",
            "Jane", "Janet", "Jasmine", "Jason", "Javier", "Jayden", "Jean", "Jeff", "Jennifer", "Jeremiah",
            "Jeremy", "Jerry", "Jesse", "Jessica", "Jian", "Joan", "Joaquin", "Joe", "Joel", "Johanna",
            "John", "Johnny", "Jonah", "Jonathan", "Jordan", "Jose", "Joseph", "Joshua", "Josiah", "Joyce",
            "Juan", "Judith", "Judy", "Julia", "Julian", "Julie", "Justin", "Kai", "Kaito", "Kali",
            "Karen", "Karina", "Kathleen", "Kathryn", "Keith", "Kelly", "Kenneth", "Kenji", "Kevin", "Kiara",
            "Kian", "Kimberly", "Kimora", "Kofi", "Kyle", "Larry", "Laura", "Lauren", "Lawrence", "Layla",
            "Leandro", "Leif", "Leilani", "Lena", "Leo", "Leon", "Leonardo", "Liam", "Liliana", "Lina",
            "Linda", "Lisa", "Logan", "Lori", "Louis", "Lucas", "Lucia", "Luis", "Luna", "Madison",
            "Maeve", "Malia", "Manuel", "Marco", "Marcus", "Maria", "Marie", "Marilyn", "Mark", "Martha",
            "Mary", "Mateo", "Matilda", "Matthew", "Maya", "Megan", "Mei", "Melina", "Melissa", "Mia",
            "Michael", "Michelle", "Mika", "Mila", "Miriam", "Mohamed", "Nancy", "Naomi", "Natalia", "Natalie",
            "Nathan", "Nicholas", "Nicole", "Niamh", "Nia", "Noah", "Nyla", "Olivia", "Omar", "Oscar",
            "Pablo", "Pamela", "Paolo", "Patricia", "Patrick", "Paul", "Penelope", "Perla", "Peter", "Petra",
            "Philip", "Phoebe", "Priya", "Rachel", "Rafael", "Rahul", "Raina", "Ralph", "Ramiro", "Ramon",
            "Randy", "Raphael", "Raul", "Ray", "Rebecca", "Ren", "Ricardo", "Richard", "Rina", "Rio",
            "Rishi", "Robert", "Roberto", "Roger", "Ronald", "Rosa", "Rosalia", "Rose", "Rowan", "Roy",
            "Ruben", "Rui", "Rumi", "Russell", "Ruth", "Ryan", "Sage", "Salma", "Samantha", "Samuel",
            "Sandra", "Santiago", "Sara", "Sarah", "Sasha", "Scott", "Sean", "Sebastian", "Selena", "Seraphina",
            "Sharon", "Siobhan", "Sofia", "Sol", "Sophia", "Sora", "Stefan", "Stephanie", "Stephen", "Stella",
            "Steven", "Susan", "Talia", "Tamara", "Tariq", "Teresa", "Terry", "Theresa", "Theron", "Thiago",
            "Thomas", "Tiana", "Tyler", "Ulyana", "Valentina", "Valeria", "Vanessa", "Vera", "Victor", "Victoria",
            "Vincent", "Violeta", "Virginia", "Vivian", "Walter", "Wayne", "Wei", "William", "Willie", "Willow",
            "Ximena", "Yara", "Yasmin", "Yasin", "Yoko", "Yolanda", "Youssef", "Yu", "Zachary", "Zain",
            "Zara", "Zayd", "Zephyr", "Zola", "Zuri"
        };

        private static readonly List<string> LastNames = new List<string>
        {
            "Abdullah", "Adams", "Aguilar", "Ahmed", "Alexander", "Ali", "Allen", "Alvarez", "Andersen", "Anderson",
            "Armstrong", "Arnold", "Bailey", "Baker", "Bakker", "Barnes", "Bauer", "Bell", "Bennett", "Bernard",
            "Black", "Brooks", "Brown", "Bryant", "Burke", "Burns", "Butler", "Campbell", "Carpenter", "Carter",
            "Castillo", "Castro", "Chapman", "Chang", "Chavez", "Chen", "Clark", "Cohen", "Coleman", "Collins",
            "Conti", "Cook", "Cooper", "Costa", "Cox", "Crawford", "Cruz", "Cunningham", "Da Silva", "Davis",
            "Day", "De Jong", "De Vries", "Dean", "Diaz", "Dubois", "Edwards", "Ellis", "Eriksson", "Esposito",
            "Evans", "Fernandez", "Ferrari", "Ferreira", "Fisher", "Fitzpatrick", "Flores", "Ford", "Foster", "Fox",
            "Franklin", "Freeman", "Garcia", "Gardner", "Garza", "George", "Gibson", "Goldberg", "Gomez", "Gonzalez",
            "Graham", "Grant", "Gray", "Green", "Greene", "Griffin", "Gupta", "Gutierrez", "Guzman", "Hall",
            "Hamilton", "Hansen", "Harris", "Harrison", "Hart", "Hassan", "Hayes", "Henderson", "Henry", "Hernandez",
            "Herrera", "Hicks", "Hill", "Howard", "Hughes", "Hunter", "Ivanov", "Jackson", "James", "Janssen",
            "Jenkins", "Jensen", "Jimenez", "Johansson", "Johnson", "Johnston", "Jones", "Jordan", "Karlsson", "Kaur",
            "Kelly", "Kennedy", "Khan", "Kim", "King", "Kowalski", "Kristiansen", "Kumar", "Lawson", "Lawrence",
            "Lee", "Leroy", "Levy", "Lewandowski", "Lewis", "Li", "Lin", "Lindberg", "Long", "Lopez",
            "Lynch", "MacLeod", "Mahmoud", "Marshall", "Martin", "Martinez", "Matthews", "Mcdonald", "Medina", "Mendoza",
            "Meyer", "Miller", "Mills", "Mitchell", "Moore", "Morales", "Moreau", "Morgan", "Morris", "Muller",
            "Murphy", "Murray", "Myers", "Nelson", "Nguyen", "Nielsen", "Nowak", "O'Connell", "Oliveira", "Olsen",
            "Olson", "Ortiz", "Owens", "Park", "Parker", "Patel", "Patterson", "Pereira", "Perez", "Perry",
            "Peterson", "Petrova", "Pham", "Phillips", "Popov", "Porter", "Powell", "Price", "Ramirez", "Ramos",
            "Ray", "Reed", "Reyes", "Reynolds", "Rice", "Richardson", "Richter", "Riley", "Rivera", "Roberts",
            "Robinson", "Rodriguez", "Rogers", "Romano", "Romero", "Rose", "Ross", "Rossi", "Ruiz", "Russell",
            "Sanchez", "Sanders", "Santos", "Sato", "Schmidt", "Schneider", "Scott", "Sharma", "Simmons", "Simpson",
            "Singh", "Smirnov", "Smith", "Snyder", "Stevens", "Stewart", "Stone", "Sullivan", "Suzuki", "Svensson",
            "Tanaka", "Taylor", "Thomas", "Thompson", "Torres", "Tran", "Tucker", "Turner", "Van Der Berg", "Vargas",
            "Vasquez", "Volkov", "Wagner", "Walker", "Wallace", "Wang", "Ward", "Warren", "Washington", "Watson",
            "Webb", "Weber", "Welch", "Wells", "West", "Wheeler", "White", "Williams", "Wilson", "Wojciechowski",
            "Wong", "Wood", "Woods", "Wright", "Wu", "Young"
        };
    }
}
