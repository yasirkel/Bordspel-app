INSERT INTO Persons (FirstName, LastName, Email, DateOfBirth, Gender, Address_Street, Address_HouseNumber, Address_PostalCode, Address_City, DietaryRequirements_IsLactoseFree, DietaryRequirements_IsNutFree, DietaryRequirements_IsVegetarian, DietaryRequirements_IsAlcoholFree)
VALUES ('Yasir', 'Kel', 'yasir@email.com', '1999-03-10', 1, 'Studentenstraat', '42', '4818AB', 'Breda', 0, 0, 0, 0);

SELECT * FROM Persons WHERE Email = 'yasir@email.com';
