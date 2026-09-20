-- ============================================================================
-- SCRIPT DE SEMILLA (SEED DATA) - SISTEMA UNA MORDIDA MÁS
-- Menú extraído de Google Maps
-- ============================================================================

-- 1. CATEGORÍAS
INSERT OR IGNORE INTO Categorias (Id, Nombre, Activo) VALUES
(1, 'Cocina', 1),
(2, 'Café & Espresso', 1),
(3, 'Latte Bar', 1),
(4, 'Frappés', 1),
(5, 'Aguas Frescas', 1),
(6, 'Smoothies', 1),
(7, 'Licuados', 1),
(8, 'Fruta', 1),
(9, 'Jugos Naturales', 1);

-- 2. PRODUCTOS
-- ----------------------------------------------------------------------------
-- Categoría 1: COCINA
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
-- Quesadillas
('Quesadilla Simple', 30.00, 1, 1),
('Quesadilla con Jamón', 40.00, 1, 1),
('Quesadilla con Pierna', 40.00, 1, 1),
('Quesadilla con Machaca', 40.00, 1, 1),

-- Tortas
('Torta de Jamón y Queso', 70.00, 1, 1),
('Torta de Pierna Sencilla', 75.00, 1, 1),
('Torta de Pierna Especial c/ Jamón y Queso', 95.00, 1, 1),

-- Huevos al gusto ($100)
('Huevos Revueltos al Gusto', 100.00, 1, 1),
('Huevos Estrellados al Gusto', 100.00, 1, 1),
('Omelette de Huevo', 100.00, 1, 1),
('Huevos con Tocino', 100.00, 1, 1),
('Huevos con Jamón', 100.00, 1, 1),
('Huevos a la Mexicana', 100.00, 1, 1),

-- Burritos
('Burrito de Huevo con Chorizo', 30.00, 1, 1),
('Burrito de Machaca', 35.00, 1, 1),
('Burrito de Pierna', 35.00, 1, 1),
('Burrito de Machaca con Huevo', 40.00, 1, 1),

-- Sandwiches
('Sandwich de Atún', 60.00, 1, 1),
('Sandwich de Jamón', 60.00, 1, 1),
('Sandwich de Pechuga de Pollo', 70.00, 1, 1),
('Sandwich de Pierna', 70.00, 1, 1),

-- Chilaquiles
('Chilaquiles con Huevo', 65.00, 1, 1),
('Chilaquiles con Pollo', 75.00, 1, 1);

-- ----------------------------------------------------------------------------
-- Categoría 2: CAFÉ & ESPRESSO
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Espresso 1 oz', 40.00, 2, 1),
('Espresso Doble 2 oz', 45.00, 2, 1),
('Macchiato 1 oz', 40.00, 2, 1),
('Macchiato Doble 2 oz', 45.00, 2, 1),
('Americano 16 oz', 50.00, 2, 1),
('Espresso Americano 16 oz', 50.00, 2, 1),
('Cappuccino 16 oz', 65.00, 2, 1),
('Cappuccino Avellana 16 oz', 75.00, 2, 1),
('Cappuccino Vainilla Francesa 16 oz', 75.00, 2, 1),
('Cappuccino Caramelo 16 oz', 75.00, 2, 1),
('Cappuccino Crema Irlandesa 16 oz', 75.00, 2, 1),
('Mocha 16 oz', 75.00, 2, 1),
('Dirty Chai 16 oz', 75.00, 2, 1);

-- ----------------------------------------------------------------------------
-- Categoría 3: LATTE BAR
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Latte 16 oz', 60.00, 3, 1),
('Latte Avellana 16 oz', 75.00, 3, 1),
('Latte Vainilla Francesa 16 oz', 75.00, 3, 1),
('Latte Caramelo 16 oz', 75.00, 3, 1),
('Latte Crema Irlandesa 16 oz', 75.00, 3, 1),
('Dirty Chai Latte 16 oz', 75.00, 3, 1),
('Mocha Latte 16 oz', 75.00, 3, 1),
('Chai Latte 16 oz', 70.00, 3, 1),
('Chocolate Caliente 16 oz', 65.00, 3, 1);

-- ----------------------------------------------------------------------------
-- Categoría 4: FRAPPÉS (16 oz)
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Frappuccino 16 oz', 70.00, 4, 1),
('Frappé Mocha 16 oz', 80.00, 4, 1),
('Frappé Avellana 16 oz', 80.00, 4, 1),
('Frappé Vainilla Francesa 16 oz', 80.00, 4, 1),
('Frappé Caramelo 16 oz', 80.00, 4, 1),
('Frappé Crema Irlandesa 16 oz', 80.00, 4, 1),
('Frappé Dirty Chai 16 oz', 80.00, 4, 1),
('Frappé Cookies & Cream 16 oz', 80.00, 4, 1),
('Frappé Chai 16 oz', 70.00, 4, 1),
('Frappé Chocolate 16 oz', 70.00, 4, 1);

-- ----------------------------------------------------------------------------
-- Categoría 5: AGUAS FRESCAS
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Agua de Sandía 1/2 L', 30.00, 5, 1),
('Agua de Sandía 1 L', 60.00, 5, 1),
('Agua de Melón 1/2 L', 30.00, 5, 1),
('Agua de Melón 1 L', 60.00, 5, 1),
('Agua de Papaya 1/2 L', 30.00, 5, 1),
('Agua de Papaya 1 L', 60.00, 5, 1),
('Agua de Limón 1/2 L', 30.00, 5, 1),
('Agua de Limón 1 L', 60.00, 5, 1),
('Agua de Naranja 1/2 L', 30.00, 5, 1),
('Agua de Naranja 1 L', 60.00, 5, 1),
('Agua de Fresa 1/2 L', 30.00, 5, 1),
('Agua de Fresa 1 L', 60.00, 5, 1),
('Agua de Piña 1/2 L', 30.00, 5, 1),
('Agua de Piña 1 L', 60.00, 5, 1),
('Agua de Guayaba 1/2 L', 30.00, 5, 1),
('Agua de Guayaba 1 L', 60.00, 5, 1),
('Agua de Pepino c/ Limón 1/2 L', 30.00, 5, 1),
('Agua de Pepino c/ Limón 1 L', 60.00, 5, 1),
('Agua de Mango 1/2 L', 30.00, 5, 1),
('Agua de Mango 1 L', 60.00, 5, 1);

-- ----------------------------------------------------------------------------
-- Categoría 6: SMOOTHIES ($100)
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Smoothie de Fresa', 100.00, 6, 1),
('Smoothie de Mango', 100.00, 6, 1),
('Smoothie de Piña', 100.00, 6, 1),
('Smoothie de Sandía', 100.00, 6, 1),
('Smoothie de Melón', 100.00, 6, 1),
('Smoothie de Papaya', 100.00, 6, 1),
('Smoothie de Guayaba', 100.00, 6, 1);

-- ----------------------------------------------------------------------------
-- Categoría 7: LICUADOS
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Licuado de Fresa 1/2 L', 50.00, 7, 1),
('Licuado de Fresa 1 L', 95.00, 7, 1),
('Licuado de Fresa c/ Yogurt', 100.00, 7, 1),
('Licuado de Plátano 1/2 L', 50.00, 7, 1),
('Licuado de Plátano 1 L', 95.00, 7, 1),
('Licuado de Plátano c/ Yogurt', 100.00, 7, 1),
('Licuado de Papaya 1/2 L', 50.00, 7, 1),
('Licuado de Papaya 1 L', 95.00, 7, 1),
('Licuado de Papaya c/ Yogurt', 100.00, 7, 1),
('Licuado de Guayaba 1/2 L', 50.00, 7, 1),
('Licuado de Guayaba 1 L', 95.00, 7, 1),
('Licuado de Guayaba c/ Yogurt', 100.00, 7, 1),
('Licuado Plátano c/ Chocolate 1/2 L', 50.00, 7, 1),
('Licuado Plátano c/ Chocolate 1 L', 95.00, 7, 1),
('Licuado Plátano c/ Chocolate c/ Yogurt', 100.00, 7, 1),
('Licuado Frutos Rojos c/ Avena 1/2 L', 50.00, 7, 1),
('Licuado Frutos Rojos c/ Avena 1 L', 95.00, 7, 1),
('Licuado Frutos Rojos c/ Avena c/ Yogurt', 100.00, 7, 1),
('Licuado de Mango 1/2 L', 50.00, 7, 1),
('Licuado de Mango 1 L', 95.00, 7, 1),
('Licuado de Mango c/ Yogurt', 100.00, 7, 1),
('Chocomilk 1/2 L', 50.00, 7, 1),
('Chocomilk 1 L', 95.00, 7, 1);

-- ----------------------------------------------------------------------------
-- Categoría 8: FRUTA
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Coctel de Frutas Mini', 30.00, 8, 1),
('Coctel de Frutas Grande', 50.00, 8, 1),
('Fruta con Yogurt Chico', 40.00, 8, 1),
('Fruta con Yogurt Grande', 75.00, 8, 1);

-- ----------------------------------------------------------------------------
-- Categoría 9: JUGOS NATURALES
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Productos (Nombre, Precio, CategoriaId, Activo) VALUES
('Jugo de Naranja 1/2 L', 50.00, 9, 1),
('Jugo de Naranja 1 L', 100.00, 9, 1),
('Jugo de Zanahoria 1/2 L', 50.00, 9, 1),
('Jugo de Zanahoria 1 L', 100.00, 9, 1),
('Jugo Combinado Naranja/Zanahoria/Betabel 1/2 L', 50.00, 9, 1),
('Jugo Combinado Naranja/Zanahoria/Betabel 1 L', 100.00, 9, 1);

-- ----------------------------------------------------------------------------
-- 3. EXTRAS / MODIFICADORES
-- ----------------------------------------------------------------------------
INSERT OR IGNORE INTO Extras (Id, Nombre, Precio, Activo) VALUES
(1, 'Queso Extra', 15.00, 1),
(2, 'Tocino Extra', 20.00, 1),
(3, 'Sin Cebolla', 0.00, 1),
(4, 'Shot Extra Espresso', 12.00, 1),
(5, 'Leche Deslactosada', 10.00, 1),
(6, 'Leche de Almendras', 12.00, 1),
(7, 'Jarabe de Vainilla', 10.00, 1),
(8, 'Jarabe de Caramelo', 10.00, 1),
(9, 'Preparar Frío', 0.00, 1),
(10, 'Preparar Caliente', 0.00, 1),
(11, 'Agregar Betabel', 10.00, 1),
(12, 'Agregar Avena', 10.00, 1),
(13, 'Sin Azúcar', 0.00, 1),
(14, 'Para Llevar', 0.00, 1);

-- ----------------------------------------------------------------------------
-- 4. VALIDACIÓN DE EXTRAS POR CATEGORÍA (CategoriaExtras)
-- ----------------------------------------------------------------------------
-- Extras para Cocina (Categoría 1)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(1, 1),  -- Queso Extra
(1, 2),  -- Tocino Extra
(1, 3),  -- Sin Cebolla
(1, 14); -- Para Llevar

-- Extras para Café & Espresso (Categoría 2)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(2, 4),  -- Shot Extra
(2, 5),  -- Leche Deslactosada
(2, 6),  -- Leche de Almendras
(2, 7),  -- Jarabe Vainilla
(2, 8),  -- Jarabe Caramelo
(2, 9),  -- Preparar Frío
(2, 10), -- Preparar Caliente
(2, 14); -- Para Llevar

-- Extras para Latte Bar (Categoría 3)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(3, 4),  -- Shot Extra
(3, 5),  -- Leche Deslactosada
(3, 6),  -- Leche de Almendras
(3, 7),  -- Jarabe Vainilla
(3, 8),  -- Jarabe Caramelo
(3, 9),  -- Preparar Frío
(3, 10), -- Preparar Caliente
(3, 14); -- Para Llevar

-- Extras para Frappés (Categoría 4)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(4, 4),  -- Shot Extra
(4, 5),  -- Leche Deslactosada
(4, 6),  -- Leche de Almendras
(4, 7),  -- Jarabe Vainilla
(4, 8),  -- Jarabe Caramelo
(4, 14); -- Para Llevar

-- Extras para Aguas Frescas (Categoría 5)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(5, 13), -- Sin Azúcar
(5, 14); -- Para Llevar

-- Extras para Smoothies (Categoría 6)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(6, 13), -- Sin Azúcar
(6, 14); -- Para Llevar

-- Extras para Licuados (Categoría 7)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(7, 5),  -- Leche Deslactosada
(7, 6),  -- Leche de Almendras
(7, 12), -- Agregar Avena
(7, 13), -- Sin Azúcar
(7, 14); -- Para Llevar

-- Extras para Jugos Naturales (Categoría 9)
INSERT OR IGNORE INTO CategoriaExtras (CategoriaId, ExtraId) VALUES
(9, 11), -- Agregar Betabel
(9, 13), -- Sin Azúcar
(9, 14); -- Para Llevar
