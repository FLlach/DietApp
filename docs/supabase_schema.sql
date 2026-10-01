-- =============================================================================
-- DietApp - Esquema Relacional de Base de Datos para Supabase (PostgreSQL)
-- =============================================================================
-- Ejecuta este script en el SQL Editor de tu proyecto en Supabase (https://supabase.com/dashboard)
-- Crea las tablas con sus relaciones foraneas, indices B-Tree para consultas de minerales
-- y politicas de seguridad RLS (Row Level Security).
-- =============================================================================

-- Habilitar extension pgcrypto o uuid-ossp si no estan activadas
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- -----------------------------------------------------------------------------
-- 1. Tabla: user_profiles (Perfiles y Preferencias de Pacientes)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.user_profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL DEFAULT 'Usuario Principal',
    email TEXT,
    dietary_condition TEXT NOT NULL DEFAULT 'Renal / KDOQI',
    weight_kg DOUBLE PRECISION,
    height_cm DOUBLE PRECISION,
    birth_date TIMESTAMP WITH TIME ZONE,
    biological_sex TEXT,
    daily_calorie_target DOUBLE PRECISION NOT NULL DEFAULT 2000.0,
    is_calorie_goal_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    daily_protein_target_grams DOUBLE PRECISION NOT NULL DEFAULT 60.0,
    is_protein_goal_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    warning_threshold_percentage DOUBLE PRECISION NOT NULL DEFAULT 80.0,
    potassium_limit_mg DOUBLE PRECISION NOT NULL DEFAULT 2000.0,
    is_potassium_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    phosphorus_limit_mg DOUBLE PRECISION NOT NULL DEFAULT 850.0,
    is_phosphorus_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    sodium_limit_mg DOUBLE PRECISION NOT NULL DEFAULT 1500.0,
    is_sodium_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    calcium_limit_mg DOUBLE PRECISION NOT NULL DEFAULT 1000.0,
    is_calcium_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    magnesium_limit_mg DOUBLE PRECISION NOT NULL DEFAULT 350.0,
    is_magnesium_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    iron_limit_mg DOUBLE PRECISION NOT NULL DEFAULT 15.0,
    is_iron_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    zinc_limit_mg DOUBLE PRECISION NOT NULL DEFAULT 12.0,
    is_zinc_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    preferred_language TEXT NOT NULL DEFAULT 'es',
    theme_preference TEXT NOT NULL DEFAULT 'Dark',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Indice para busquedas inmediatas por user_id
CREATE INDEX IF NOT EXISTS idx_user_profiles_user_id ON public.user_profiles(user_id);

-- -----------------------------------------------------------------------------
-- 2. Tabla: foods (Catalogo de Alimentos y Densidad Mineral)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.foods (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    category TEXT NOT NULL DEFAULT '',
    reference_grams DOUBLE PRECISION NOT NULL DEFAULT 100.0,
    calories DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    protein_grams DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    phosphorus_mg DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    potassium_mg DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    sodium_mg DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calcium_mg DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    magnesium_mg DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    iron_mg DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    zinc_mg DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    barcode TEXT,
    is_custom BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_foods_name ON public.foods(name);
CREATE INDEX IF NOT EXISTS idx_foods_barcode ON public.foods(barcode);
CREATE INDEX IF NOT EXISTS idx_foods_potassium ON public.foods(potassium_mg);
CREATE INDEX IF NOT EXISTS idx_foods_phosphorus ON public.foods(phosphorus_mg);
CREATE INDEX IF NOT EXISTS idx_foods_sodium ON public.foods(sodium_mg);

-- -----------------------------------------------------------------------------
-- 3. Tabla: meals (Registro Diario de Ingestas)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.meals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id TEXT NOT NULL DEFAULT 'local_user',
    date DATE NOT NULL,
    meal_type_value INTEGER NOT NULL,
    note TEXT NOT NULL DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_meals_user_date ON public.meals(user_id, date);

-- -----------------------------------------------------------------------------
-- 4. Tabla: meal_items (Items consumidos en cada comida)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.meal_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    meal_id UUID NOT NULL REFERENCES public.meals(id) ON DELETE CASCADE,
    food_item_id UUID,
    food_name TEXT NOT NULL,
    grams DOUBLE PRECISION NOT NULL,
    calculated_calories DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_protein DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    recipe_id UUID,
    servings_consumed DOUBLE PRECISION NOT NULL DEFAULT 1.0,
    is_recipe BOOLEAN NOT NULL DEFAULT FALSE,
    minerals_json TEXT NOT NULL DEFAULT '[]'
);

CREATE INDEX IF NOT EXISTS idx_meal_items_meal_id ON public.meal_items(meal_id);

-- -----------------------------------------------------------------------------
-- 5. Tabla: recipes (Recetas Culinarias del Usuario)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.recipes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id TEXT NOT NULL DEFAULT 'local_user',
    title TEXT NOT NULL,
    servings INTEGER NOT NULL DEFAULT 1,
    image_path TEXT NOT NULL DEFAULT '',
    notes TEXT NOT NULL DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_recipes_user_id ON public.recipes(user_id);

-- -----------------------------------------------------------------------------
-- 6. Tabla: recipe_ingredients (Ingredientes dosificados de la receta)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.recipe_ingredients (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    recipe_id UUID NOT NULL REFERENCES public.recipes(id) ON DELETE CASCADE,
    food_item_id UUID NOT NULL,
    food_name TEXT NOT NULL,
    grams DOUBLE PRECISION NOT NULL,
    calculated_calories DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_protein DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    minerals_json TEXT NOT NULL DEFAULT '[]'
);

CREATE INDEX IF NOT EXISTS idx_recipe_ingredients_recipe_id ON public.recipe_ingredients(recipe_id);

-- -----------------------------------------------------------------------------
-- 7. Tabla: recipe_steps (Pasos de elaboracion de la receta)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.recipe_steps (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    recipe_id UUID NOT NULL REFERENCES public.recipes(id) ON DELETE CASCADE,
    step_number INTEGER NOT NULL,
    instruction TEXT NOT NULL,
    image_path TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS idx_recipe_steps_recipe_id ON public.recipe_steps(recipe_id);

-- -----------------------------------------------------------------------------
-- 8. Seguridad: Politicas RLS (Row Level Security)
-- -----------------------------------------------------------------------------
-- Habilitar RLS en las tablas principales
ALTER TABLE public.user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.foods ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.meals ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.meal_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recipes ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recipe_ingredients ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recipe_steps ENABLE ROW LEVEL SECURITY;

-- Politica permisiva para fase de desarrollo / anon key
-- Permite lectura y escritura a clientes con la Anon Key valida
CREATE POLICY "Permitir acceso completo a perfiles" ON public.user_profiles FOR ALL USING (true) WITH CHECK (true);
CREATE POLICY "Permitir lectura publica de alimentos" ON public.foods FOR SELECT USING (true);
CREATE POLICY "Permitir escritura de alimentos" ON public.foods FOR ALL USING (true) WITH CHECK (true);
CREATE POLICY "Permitir acceso completo a comidas" ON public.meals FOR ALL USING (true) WITH CHECK (true);
CREATE POLICY "Permitir acceso completo a items de comidas" ON public.meal_items FOR ALL USING (true) WITH CHECK (true);
CREATE POLICY "Permitir acceso completo a recetas" ON public.recipes FOR ALL USING (true) WITH CHECK (true);
CREATE POLICY "Permitir acceso completo a ingredientes" ON public.recipe_ingredients FOR ALL USING (true) WITH CHECK (true);
CREATE POLICY "Permitir acceso completo a pasos" ON public.recipe_steps FOR ALL USING (true) WITH CHECK (true);

-- -----------------------------------------------------------------------------
-- 9. Privilegios de Acceso PostgreSQL (Roles: anon y authenticated)
-- -----------------------------------------------------------------------------
GRANT USAGE ON SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL TABLES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL ROUTINES IN SCHEMA public TO anon, authenticated;

ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO anon, authenticated;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO anon, authenticated;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON ROUTINES TO anon, authenticated;

