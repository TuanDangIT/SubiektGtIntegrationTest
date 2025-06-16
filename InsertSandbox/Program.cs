using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InsERT;

namespace InsertSandbox
{
    internal class Program
    {
        //It is important to note that application must single threaded, otherwise it will not work properly.
        [STAThread]
        static void Main(string[] args)
        {
            Subiekt subiekt = TurnOnSubiekt();
            if (subiekt != null)
            {
                Console.WriteLine("Subiekt GT launched successfully.");
            }
            else
            {
                Console.WriteLine("Failed to start Subiekt GT.");
                return;
            }
            while (true)
            {
                Console.WriteLine("Choose one of the following actions:");
                Console.WriteLine("1. Add a new customer");
                Console.WriteLine("2. Add Document (ZK, PS, FS)");
                Console.WriteLine("3. Delete a document (ZK, PS, FS) by ID");
                Console.WriteLine("4. Add product");
                Console.WriteLine("5. Delete product by ID");
                Console.WriteLine("\"exit\". End console app");
                var chosenAction = Console.ReadLine();
                try
                {
                    switch (chosenAction)
                    {
                        case "1":
                            AddDemoCustomer(subiekt);
                            break;
                        case "2":
                            CreateSuDocument(subiekt);
                            break;
                        case "3":
                            DeleteZk(subiekt);
                            break;
                        case "4":
                            AddProduct(subiekt);
                            break;
                        case "5":
                            DeleteProduct(subiekt);
                            break;
                        case "exit":
                            return;
                        default:
                            Console.WriteLine("Invalid action. Please try again.");
                            continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    continue;
                }
            }
        }
        static Subiekt TurnOnSubiekt()
        {
            GT gt = new GT();
            gt.Produkt = ProduktEnum.gtaProduktSubiekt;
            gt.Autentykacja = AutentykacjaEnum.gtaAutentykacjaMieszana;
            gt.Baza = "test";
            gt.Operator = "Szef";
            gt.Serwer = "(local)\\INSERTGT";
            //gt.Baza = Konf.MsSql_Subiekt.Baza;
            //gt.Autentykacja = InsERT.AutentykacjaEnum.gtaAutentykacjaMieszana;
            //gt.Uzytkownik = Konf.MsSql_Subiekt.Login;
            //gt.UzytkownikHaslo = Konf.MsSql_Subiekt.Haslo;
            //gt.Operator = Konf.Subiekt.Operator;
            //gt.OperatorHaslo = Konf.Subiekt.OperatorHaslo;
            // Uruchomienie Subiekta GT
            Subiekt subiekt = (Subiekt)gt.Uruchom((Int32)UruchomDopasujEnum.gtaUruchomDopasuj, (Int32)UruchomEnum.gtaUruchomNieArchiwizujPrzyZamykaniu);
            subiekt.Okno.Widoczne = false; // true = subiekt on display - false= subiekt in background
            return subiekt;
        }

        static void CreateSuDocument(Subiekt subiekt)
        {
            Console.WriteLine("Enter type of document");
            string inputDocumentType = Console.ReadLine();
            SubiektDokumentEnum documentType = default; //Initialize document type variable
            switch (inputDocumentType)
            {
                case "ZK":
                    documentType = SubiektDokumentEnum.gtaSubiektDokumentZK;
                    break;
                case "PA":
                    documentType = SubiektDokumentEnum.gtaSubiektDokumentPA;
                    break;
                case "FS":
                    documentType = SubiektDokumentEnum.gtaSubiektDokumentFS;
                    break;
                default:
                    throw new Exception($"Document type: {inputDocumentType} is not supported. Supported types are: ZK, PA, FS.");
            }
            SuDokument document = subiekt.Dokumenty.Dodaj((int)documentType); //Create new document of specified type
            Console.WriteLine($"Enter customer symbol for the new {inputDocumentType}:");
            var customerSymbol = Console.ReadLine();
            Kontrahent customer = subiekt.Kontrahenci.Wczytaj(customerSymbol) ?? throw new Exception($"Customer with symbol: {customerSymbol} was not found."); //Read customer by symbol
            document.KontrahentId = customer.Identyfikator;
            while (true)
            {
                Console.WriteLine($"Enter the symbol of the product to add to the new {inputDocumentType} or type 'exit' to finish:");
                string symbol = Console.ReadLine();
                if (symbol == "exit")
                {
                    break;
                }
                var item = subiekt.Towary.Wczytaj(symbol); //Read item by symbol
                if (item is null)
                {
                    Console.WriteLine($"Product with symbol: {symbol} was not found. Please try again.");
                    continue;
                }
                Console.WriteLine("Enter amount of the product");
                if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0)
                {
                    Console.WriteLine("Invalid amount. Please enter a positive integer.");
                    continue;
                }
                SuPozycja documentItem = document.Pozycje.Dodaj(item); //Add item to document
                documentItem.IloscJm = amount; //Set amount of the item
            }
            document.DataWystawienia = DateTime.Now; //Set issue date of the document
            document.Zapisz(); //Save the document
            Console.WriteLine("Sales invoice added successfully.");
        }

        static void DeleteZk(Subiekt subiekt)
        {
            Console.WriteLine("Enter the ID of the sales invoice (ZK, PS, FS) to delete:");
            var documentId = Console.ReadLine();
            SuDokument document = subiekt.Dokumenty.Wczytaj(documentId) ?? throw new Exception($"Zk: {documentId} was not found."); //Read document by ID
            document.Usun();
            Console.WriteLine($"ZK: {documentId} was deleted successfully.");
        }
        static void AddDemoCustomer(Subiekt subiekt)
        {
            Kontrahent customer = subiekt.Kontrahenci.Dodaj();
            customer.Symbol = "Symbol"; 
            customer.NazwaPelna = "Full Name";
            customer.Nazwa = "Short Name";
            customer.Zapisz(); // Save the customer
            Console.WriteLine("New customer added successfully.");
        }

        static void AddProduct(Subiekt subiekt)
        {
            Console.WriteLine("Enter a unique symbol for the product:");
            var uniqueSymbol = Console.ReadLine();
            Console.WriteLine("Enter the name of the product:");
            var productName = Console.ReadLine();
            Towar product = subiekt.Towary.Dodaj((int)TowarRodzajEnum.gtaTowarRodzajTowar);
            product.Symbol = uniqueSymbol;
            product.Nazwa = productName;
            product.Zapisz(); // Save the product
            Console.WriteLine($"Product '{productName}' with symbol '{uniqueSymbol}' added successfully.");
        }

        static void DeleteProduct(Subiekt subiekt)
        {
            Console.WriteLine("Enter the symbol of the product to delete:");
            var productSymbol = Console.ReadLine();
            Towar towar = subiekt.Towary.Wczytaj(productSymbol);
            towar.Usun(); // Delete the product by symbol
            Console.WriteLine($"Product with symbol '{productSymbol}' deleted successfully.");
        }
    }
}
