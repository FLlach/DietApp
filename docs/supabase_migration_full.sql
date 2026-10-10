-- =============================================================================
-- DietApp - Migracion Integral y Esquema Relacional Supabase (PostgreSQL)
-- =============================================================================
-- Incluye:
-- 1. Esquema DDL con extensiones, tablas, relaciones, claves foraneas e indices
-- 2. Sistema de autenticacion con auth.users, trigger de creacion automatica de perfil
-- 3. Politicas de seguridad Row Level Security (RLS) segun mejores practicas
-- 4. Migracion completa de datos locales: 363 alimentos, 383 porciones y recetas base
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- -----------------------------------------------------------------------------
-- 1. Tabla: user_profiles (Perfiles y preferencias vinculados a auth.users)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.user_profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL UNIQUE REFERENCES auth.users(id) ON DELETE CASCADE,
    display_name TEXT NOT NULL DEFAULT 'Usuario',
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

CREATE INDEX IF NOT EXISTS idx_user_profiles_user_id ON public.user_profiles(user_id);

-- Funcion trigger para sincronizacion automatica al registrar usuario en Supabase Auth
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = ''
AS $$
BEGIN
    INSERT INTO public.user_profiles (user_id, email, display_name)
    VALUES (
        NEW.id,
        NEW.email,
        COALESCE(
            NEW.raw_user_meta_data->>'display_name',
            NEW.raw_user_meta_data->>'full_name',
            NEW.raw_user_meta_data->>'name',
            split_part(NEW.email, '@', 1),
            'Usuario'
        )
    )
    ON CONFLICT (user_id) DO NOTHING;
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- -----------------------------------------------------------------------------
-- 2. Tabla: foods (Catalogo de Alimentos y Densidad Nutricional)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.foods (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    fdc_id INTEGER,
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
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_foods_name ON public.foods(name);
CREATE INDEX IF NOT EXISTS idx_foods_barcode ON public.foods(barcode);
CREATE INDEX IF NOT EXISTS idx_foods_category ON public.foods(category);
CREATE INDEX IF NOT EXISTS idx_foods_potassium ON public.foods(potassium_mg);
CREATE INDEX IF NOT EXISTS idx_foods_phosphorus ON public.foods(phosphorus_mg);
CREATE INDEX IF NOT EXISTS idx_foods_sodium ON public.foods(sodium_mg);
CREATE INDEX IF NOT EXISTS idx_foods_protein ON public.foods(protein_grams);
CREATE INDEX IF NOT EXISTS idx_foods_user_id ON public.foods(user_id);

-- -----------------------------------------------------------------------------
-- 3. Tabla: food_portions (Porciones habituales con peso normalizado en gramos)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.food_portions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    food_id UUID NOT NULL REFERENCES public.foods(id) ON DELETE CASCADE,
    measure_unit_name TEXT NOT NULL,
    amount DOUBLE PRECISION NOT NULL,
    gram_weight DOUBLE PRECISION NOT NULL,
    modifier TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS idx_food_portions_food_id ON public.food_portions(food_id);

-- -----------------------------------------------------------------------------
-- 4. Tabla: meals (Registro de ingestas diarias por usuario)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.meals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    meal_type_value INTEGER NOT NULL,
    note TEXT NOT NULL DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_meals_user_date ON public.meals(user_id, date);

-- -----------------------------------------------------------------------------
-- 5. Tabla: meal_items (Items dosificados e instantanea nutricional consumida)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.meal_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meal_id UUID NOT NULL REFERENCES public.meals(id) ON DELETE CASCADE,
    food_item_id UUID,
    food_name TEXT NOT NULL,
    portion_in_grams DOUBLE PRECISION NOT NULL,
    calculated_calories DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_protein DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_minerals_json TEXT NOT NULL DEFAULT '[]'
);

CREATE INDEX IF NOT EXISTS idx_meal_items_meal_id ON public.meal_items(meal_id);

-- -----------------------------------------------------------------------------
-- 6. Tabla: recipes (Recetas culinarias)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.recipes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
    title TEXT NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    servings INTEGER NOT NULL DEFAULT 1,
    final_image_path TEXT NOT NULL DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_recipes_user_id ON public.recipes(user_id);

-- -----------------------------------------------------------------------------
-- 7. Tabla: recipe_ingredients (Ingredientes de las recetas)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.recipe_ingredients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    recipe_id UUID NOT NULL REFERENCES public.recipes(id) ON DELETE CASCADE,
    food_item_id UUID,
    food_name TEXT NOT NULL,
    grams DOUBLE PRECISION NOT NULL,
    calculated_calories DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_protein DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_minerals_json TEXT NOT NULL DEFAULT '[]'
);

CREATE INDEX IF NOT EXISTS idx_recipe_ingredients_recipe_id ON public.recipe_ingredients(recipe_id);

-- -----------------------------------------------------------------------------
-- 8. Tabla: recipe_steps (Pasos de preparacion de recetas)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.recipe_steps (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    recipe_id UUID NOT NULL REFERENCES public.recipes(id) ON DELETE CASCADE,
    step_number INTEGER NOT NULL,
    instruction TEXT NOT NULL,
    image_path TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS idx_recipe_steps_recipe_id ON public.recipe_steps(recipe_id);

-- -----------------------------------------------------------------------------
-- 9. Tabla: seasonings (Alinos y condimentos)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.seasonings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES auth.users(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_seasonings_user_id ON public.seasonings(user_id);

-- -----------------------------------------------------------------------------
-- 10. Tabla: seasoning_items (Componentes de cada alino)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.seasoning_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    seasoning_id UUID NOT NULL REFERENCES public.seasonings(id) ON DELETE CASCADE,
    food_item_id UUID,
    food_name TEXT NOT NULL,
    grams DOUBLE PRECISION NOT NULL,
    calculated_calories DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_protein DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    calculated_minerals_json TEXT NOT NULL DEFAULT '[]'
);

CREATE INDEX IF NOT EXISTS idx_seasoning_items_seasoning_id ON public.seasoning_items(seasoning_id);

-- -----------------------------------------------------------------------------
-- 11. Seguridad: Politicas Row Level Security (RLS)
-- -----------------------------------------------------------------------------
ALTER TABLE public.user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.foods ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.food_portions ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.meals ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.meal_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recipes ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recipe_ingredients ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.recipe_steps ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.seasonings ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.seasoning_items ENABLE ROW LEVEL SECURITY;

-- Politicas para user_profiles
CREATE POLICY "user_profiles_select_own" ON public.user_profiles
    FOR SELECT TO authenticated
    USING ((select auth.uid()) = user_id);

CREATE POLICY "user_profiles_insert_own" ON public.user_profiles
    FOR INSERT TO authenticated
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "user_profiles_update_own" ON public.user_profiles
    FOR UPDATE TO authenticated
    USING ((select auth.uid()) = user_id)
    WITH CHECK ((select auth.uid()) = user_id);

-- Politicas para foods (Catalogo publico accesible a anon y authenticated, items privados por usuario)
CREATE POLICY "foods_select_public_or_own" ON public.foods
    FOR SELECT TO anon, authenticated
    USING (user_id IS NULL OR (select auth.uid()) = user_id);

CREATE POLICY "foods_insert_custom_own" ON public.foods
    FOR INSERT TO authenticated
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "foods_update_custom_own" ON public.foods
    FOR UPDATE TO authenticated
    USING ((select auth.uid()) = user_id)
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "foods_delete_custom_own" ON public.foods
    FOR DELETE TO authenticated
    USING ((select auth.uid()) = user_id);

-- Politicas para food_portions
CREATE POLICY "food_portions_select_all" ON public.food_portions
    FOR SELECT TO anon, authenticated
    USING (true);

CREATE POLICY "food_portions_insert_own" ON public.food_portions
    FOR INSERT TO authenticated
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM public.foods f
            WHERE f.id = food_portions.food_id
            AND f.user_id = (select auth.uid())
        )
    );

-- Politicas para meals y meal_items
CREATE POLICY "meals_select_own" ON public.meals
    FOR SELECT TO authenticated
    USING ((select auth.uid()) = user_id);

CREATE POLICY "meals_insert_own" ON public.meals
    FOR INSERT TO authenticated
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "meals_update_own" ON public.meals
    FOR UPDATE TO authenticated
    USING ((select auth.uid()) = user_id)
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "meals_delete_own" ON public.meals
    FOR DELETE TO authenticated
    USING ((select auth.uid()) = user_id);

CREATE POLICY "meal_items_select_own" ON public.meal_items
    FOR SELECT TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.meals m
            WHERE m.id = meal_items.meal_id
            AND m.user_id = (select auth.uid())
        )
    );

CREATE POLICY "meal_items_insert_own" ON public.meal_items
    FOR INSERT TO authenticated
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM public.meals m
            WHERE m.id = meal_items.meal_id
            AND m.user_id = (select auth.uid())
        )
    );

CREATE POLICY "meal_items_update_own" ON public.meal_items
    FOR UPDATE TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.meals m
            WHERE m.id = meal_items.meal_id
            AND m.user_id = (select auth.uid())
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM public.meals m
            WHERE m.id = meal_items.meal_id
            AND m.user_id = (select auth.uid())
        )
    );

CREATE POLICY "meal_items_delete_own" ON public.meal_items
    FOR DELETE TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.meals m
            WHERE m.id = meal_items.meal_id
            AND m.user_id = (select auth.uid())
        )
    );

-- Politicas para recipes, recipe_ingredients y recipe_steps (publicas para recetas base con user_id NULL, o propias)
CREATE POLICY "recipes_select_public_or_own" ON public.recipes
    FOR SELECT TO anon, authenticated
    USING (user_id IS NULL OR (select auth.uid()) = user_id);

CREATE POLICY "recipes_insert_own" ON public.recipes
    FOR INSERT TO authenticated
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "recipes_update_own" ON public.recipes
    FOR UPDATE TO authenticated
    USING ((select auth.uid()) = user_id)
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "recipes_delete_own" ON public.recipes
    FOR DELETE TO authenticated
    USING ((select auth.uid()) = user_id);

CREATE POLICY "recipe_ingredients_select" ON public.recipe_ingredients
    FOR SELECT TO anon, authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.recipes r
            WHERE r.id = recipe_ingredients.recipe_id
            AND (r.user_id IS NULL OR r.user_id = (select auth.uid()))
        )
    );

CREATE POLICY "recipe_ingredients_all_own" ON public.recipe_ingredients
    FOR ALL TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.recipes r
            WHERE r.id = recipe_ingredients.recipe_id
            AND r.user_id = (select auth.uid())
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM public.recipes r
            WHERE r.id = recipe_ingredients.recipe_id
            AND r.user_id = (select auth.uid())
        )
    );

CREATE POLICY "recipe_steps_select" ON public.recipe_steps
    FOR SELECT TO anon, authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.recipes r
            WHERE r.id = recipe_steps.recipe_id
            AND (r.user_id IS NULL OR r.user_id = (select auth.uid()))
        )
    );

CREATE POLICY "recipe_steps_all_own" ON public.recipe_steps
    FOR ALL TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.recipes r
            WHERE r.id = recipe_steps.recipe_id
            AND r.user_id = (select auth.uid())
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM public.recipes r
            WHERE r.id = recipe_steps.recipe_id
            AND r.user_id = (select auth.uid())
        )
    );

-- Politicas para seasonings y seasoning_items
CREATE POLICY "seasonings_select_own" ON public.seasonings
    FOR SELECT TO authenticated
    USING ((select auth.uid()) = user_id);

CREATE POLICY "seasonings_insert_own" ON public.seasonings
    FOR INSERT TO authenticated
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "seasonings_update_own" ON public.seasonings
    FOR UPDATE TO authenticated
    USING ((select auth.uid()) = user_id)
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "seasonings_delete_own" ON public.seasonings
    FOR DELETE TO authenticated
    USING ((select auth.uid()) = user_id);

CREATE POLICY "seasoning_items_select" ON public.seasoning_items
    FOR SELECT TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.seasonings s
            WHERE s.id = seasoning_items.seasoning_id
            AND s.user_id = (select auth.uid())
        )
    );

CREATE POLICY "seasoning_items_all_own" ON public.seasoning_items
    FOR ALL TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.seasonings s
            WHERE s.id = seasoning_items.seasoning_id
            AND s.user_id = (select auth.uid())
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM public.seasonings s
            WHERE s.id = seasoning_items.seasoning_id
            AND s.user_id = (select auth.uid())
        )
    );

-- -----------------------------------------------------------------------------
-- 12. Permisos y Privilegios PostgreSQL (Roles: anon y authenticated)
-- -----------------------------------------------------------------------------
GRANT USAGE ON SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL TABLES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL ROUTINES IN SCHEMA public TO anon, authenticated;

ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO anon, authenticated;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO anon, authenticated;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON ROUTINES TO anon, authenticated;


-- =============================================================================
-- MIGRACION DE DATOS LOCALES
-- =============================================================================

-- Migrando 363 Alimentos de referencia...
INSERT INTO public.foods (id, fdc_id, name, category, reference_grams, calories, protein_grams, phosphorus_mg, potassium_mg, sodium_mg, calcium_mg, magnesium_mg, iron_mg, zinc_mg, is_custom, user_id) VALUES
('1e9c9022-6f60-4cc8-ae96-068d8c1e72cb', 321358, 'Hummus, commercial', 'Legumes and Legume Products', 100.0, 229.0, 7.35, 166.0, 289.0, 438.0, 41.0, 71.1, 2.41, 1.38, FALSE, NULL),
('fbc06787-3027-4be9-9b56-f27589223534', 321360, 'Tomatoes, grape, raw', 'Vegetables and Vegetable Products', 100.0, 27.0, 0.83, 28.0, 260.0, 6.0, 11.0, 11.9, 0.33, 0.2, FALSE, NULL),
('2d3c605f-9d78-4d5a-87ba-967ac7f1fa1f', 321611, 'Beans, snap, green, canned, regular pack, drained solids', 'Vegetables and Vegetable Products', 100.0, 21.0, 1.04, 23.0, 97.0, 282.0, 36.0, 12.7, 0.78, 0.19, FALSE, NULL),
('86bc73c0-8966-4d0a-91f2-6dec9aaeeae1', 323121, 'Frankfurter, beef, unheated', 'Sausages and Luncheon Meats', 100.0, 314.0, 11.7, 128.0, 343.0, 872.0, 15.0, 11.5, 1.14, 2.06, FALSE, NULL),
('d3a4818b-eac0-4025-9022-c8287a232b8a', 323294, 'Nuts, almonds, dry roasted, with salt added', 'Nut and Seed Products', 100.0, 620.0, 20.4, 456.0, 684.0, 256.0, 273.0, 258.0, 3.17, 2.8, FALSE, NULL),
('1f01cfbc-38fa-428f-bb0d-e121ea2c6bbb', 323505, 'Kale, raw', 'Vegetables and Vegetable Products', 100.0, 35.0, 2.92, 55.0, 348.0, 53.0, 254.0, 32.7, 1.6, 0.39, FALSE, NULL),
('c1c3e7dd-1b3e-405d-9c67-985e89f0ce28', 323604, 'Egg, whole, raw, frozen, pasteurized', 'Dairy and Egg Products', 100.0, 150.0, 12.3, 189.0, 117.0, 121.0, 55.0, 11.2, 1.77, 1.2, FALSE, NULL),
('7a931c4e-ef20-4790-b395-a439af7aee7a', 323697, 'Egg, white, raw, frozen, pasteurized', 'Dairy and Egg Products', 100.0, 48.0, 10.1, 0.0, 130.0, 144.0, 9.0, 10.6, 0.18, 0.02, FALSE, NULL),
('a8f914a4-a87f-4c83-bd44-b49ef0f17600', 323793, 'Egg, white, dried', 'Dairy and Egg Products', 100.0, 376.0, 79.9, 107.0, 959.0, 1250.0, 104.0, 87.6, 0.0, 0.43, FALSE, NULL),
('1a79d64d-dc7c-45ef-b43b-a6a58bd412f8', 324317, 'Onion rings, breaded, par fried, frozen, prepared, heated in oven', 'Vegetables and Vegetable Products', 100.0, 288.0, 4.52, 75.0, 135.0, 374.0, 28.0, 20.4, 1.14, 0.59, FALSE, NULL),
('6d484f7a-247e-40c5-ab00-833412220441', 324653, 'Pickles, cucumber, dill or kosher dill', 'Vegetables and Vegetable Products', 100.0, 12.0, 0.48, 16.0, 112.0, 808.0, 54.0, 7.1, 0.23, 0.11, FALSE, NULL),
('a83e811b-4aef-4e16-bb4d-ffa0b27d0961', 325036, 'Cheese, parmesan, grated', 'Dairy and Egg Products', 100.0, 421.0, 29.6, 634.0, 184.0, 1750.0, 884.0, 34.9, 0.45, 4.33, FALSE, NULL),
('ea183dc5-b5e0-4132-8682-a96fa916782b', 325198, 'Cheese, pasteurized process, American, vitamin D fortified', 'Dairy and Egg Products', 100.0, 366.0, 18.0, 540.0, 149.0, 1660.0, 866.0, 25.1, 0.48, 2.42, FALSE, NULL),
('09e51e3a-3733-4c3c-9a55-3fadbc674b8c', 325287, 'Grapefruit juice, white, canned or bottled, unsweetened', 'Fruits and Fruit Juices', 100.0, 37.0, 0.55, 17.0, 141.0, 1.0, 16.0, 9.6, 0.07, 0.04, FALSE, NULL),
('1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 325430, 'Peaches, yellow, raw', 'Fruits and Fruit Juices', 100.0, 42.0, 0.91, 22.0, 122.0, 13.0, 4.0, 8.0, 0.34, 0.23, FALSE, NULL),
('997b7f99-0431-4731-b3d1-f7afbf49805a', 325524, 'Seeds, sunflower seed kernels, dry roasted, with salt added', 'Nut and Seed Products', 100.0, 612.0, 21.0, 750.0, 689.0, 532.0, 78.0, 358.0, 5.21, 6.0, FALSE, NULL),
('8b2448f4-197a-45ea-aa8d-a7741f20829f', 326196, 'Kale, frozen, cooked, boiled, drained, without salt', 'Vegetables and Vegetable Products', 100.0, 36.0, 2.94, 42.0, 144.0, 16.0, 150.0, 25.0, 0.84, 0.27, FALSE, NULL),
('78accd46-ad8f-4923-8513-9d53b2cbda06', 326698, 'Mustard, prepared, yellow', 'Spices and Herbs', 100.0, 61.0, 4.25, 108.0, 150.0, 1100.0, 63.0, 47.7, 1.59, 0.64, FALSE, NULL),
('c26f31cf-3d80-4e1c-816d-09c26239855e', 327046, 'Kiwifruit, green, raw', 'Fruits and Fruit Juices', 100.0, 58.0, 1.06, 34.0, 198.0, 5.0, 35.0, 15.7, 0.24, 0.14, FALSE, NULL),
('bef57fc3-fb8f-48e3-bdff-9fa16c1d2a39', 327357, 'Nectarines, raw', 'Fruits and Fruit Juices', 100.0, 39.0, 1.06, 26.0, 131.0, 13.0, 2.0, 8.8, 0.3, 0.21, FALSE, NULL),
('c9a60ea0-e148-43da-942f-121b2310d94c', 328637, 'Cheese, cheddar', 'Dairy and Egg Products', 100.0, 408.0, 23.3, 458.0, 77.0, 654.0, 707.0, 26.8, 0.16, 3.67, FALSE, NULL),
('3f48ab43-0ee3-4369-8aa8-b059145abb86', 328841, 'Cheese, cottage, lowfat, 2% milkfat', 'Dairy and Egg Products', 100.0, 84.0, 11.0, 148.0, 120.0, 321.0, 103.0, 8.9, 0.13, 0.61, FALSE, NULL),
('1589ac95-0c1f-41a2-8a8d-838bb1e8f936', 329370, 'Cheese, mozzarella, low moisture, part-skim', 'Dairy and Egg Products', 100.0, 298.0, 23.7, 533.0, 116.0, 699.0, 693.0, 27.2, 0.2, 3.62, FALSE, NULL),
('428d2964-65b4-4a76-b70e-e4c59c27b8f4', 329490, 'Egg, whole, dried', 'Dairy and Egg Products', 100.0, 575.0, 48.1, 770.0, 468.0, 485.0, 220.0, 44.6, 6.97, 5.02, FALSE, NULL),
('42459d75-369a-41c1-b7fd-29fb210f89c3', 329596, 'Egg, yolk, raw, frozen, pasteurized', 'Dairy and Egg Products', 100.0, 296.0, 15.6, 443.0, 102.0, 66.0, 119.0, 11.2, 4.13, 2.93, FALSE, NULL),
('ba8e0002-ac38-42f9-8ce9-7353e876cd2a', 329716, 'Egg, yolk, dried', 'Dairy and Egg Products', 100.0, 654.0, 34.2, 982.0, 231.0, 149.0, 270.0, 26.0, 9.28, 7.11, FALSE, NULL),
('04df975e-d4d3-45ba-8eb8-ea78106a00c1', 330137, 'Yogurt, Greek, plain, nonfat', 'Dairy and Egg Products', 100.0, 61.0, 10.3, 136.0, 141.0, 36.0, 111.0, 10.7, 0.07, 0.53, FALSE, NULL),
('bb28429f-341a-4ab8-bb42-8946c227236b', 330415, 'Yogurt, Greek, strawberry, nonfat', 'Dairy and Egg Products', 100.0, 83.0, 8.06, 113.0, 133.0, 32.0, 97.0, 10.4, 0.09, 0.46, FALSE, NULL),
('a6fb12bd-4ceb-4644-bd91-f50e11a603e2', 330458, 'Oil, coconut', 'Fats and Oils', 100.0, 833.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.05, 0.02, FALSE, NULL),
('9b24274e-eecb-424a-916b-8216bb73efef', 331897, 'Chicken, broilers or fryers, drumstick, meat only, cooked, braised', 'Poultry Products', 100.0, 156.0, 23.9, 184.0, 239.0, 117.0, 12.0, 22.4, 0.94, 2.54, FALSE, NULL),
('fb3575a0-8e89-4824-98e6-bed6412058dd', 331960, 'Chicken, broiler or fryers, breast, skinless, boneless, meat only, cooked, braised', 'Poultry Products', 100.0, 166.0, 32.1, 241.0, 343.0, 47.0, 6.0, 32.0, 0.49, 0.96, FALSE, NULL),
('e21ebea3-fffd-472b-ad84-29702473f119', 332282, 'Sauce, pasta, spaghetti/marinara, ready-to-serve', 'Soups, Sauces, and Gravies', 100.0, 45.0, 1.41, 34.0, 319.0, 419.0, 27.0, 18.5, 0.78, 0.2, FALSE, NULL),
('35d0c379-3a08-4548-b9fe-b917639c86d7', 332397, 'Ham, sliced, pre-packaged, deli meat (96%fat free, water added)', 'Sausages and Luncheon Meats', 100.0, 106.0, 16.7, 261.0, 425.0, 1040.0, 5.0, 18.8, 0.58, 1.52, FALSE, NULL),
('c80ef17d-a505-4b6f-8f6a-0cf11893631b', 332791, 'Olives, green, Manzanilla, stuffed with pimiento', 'Fruits and Fruit Juices', 100.0, 130.0, 1.15, 7.0, 43.0, 1620.0, 121.0, 12.4, 0.31, 0.09, FALSE, NULL),
('8683890f-79aa-4b0e-8d73-6b32882d4f0c', 333008, 'Cookies, oatmeal, soft, with raisins', 'Baked Products', 100.0, 430.0, 5.79, 111.0, 245.0, 314.0, 29.0, 32.4, 2.28, 0.62, FALSE, NULL),
('f51f07f4-3b62-4247-a181-b5b510541078', 333281, 'Tomatoes, canned, red, ripe, diced', 'Vegetables and Vegetable Products', 100.0, 18.0, 0.84, 18.0, 198.0, 125.0, 30.0, 9.8, 0.57, 0.12, FALSE, NULL),
('267e2083-92c9-4d25-84f2-736a3f7d44b8', 333374, 'Fish, haddock, raw', 'Finfish and Shellfish Products', 100.0, 74.0, 16.3, 227.0, 286.0, 213.0, 11.0, 21.1, 0.17, 0.32, FALSE, NULL),
('d2d8402d-9d70-4bed-b38c-4615445e92b9', 333476, 'Fish, pollock, raw', 'Finfish and Shellfish Products', 100.0, 56.0, 12.3, 284.0, 160.0, 333.0, 15.0, 16.1, 0.22, 0.31, FALSE, NULL),
('4de7d3fa-e504-4e7a-b68e-421d565bc761', 334194, 'Fish, tuna, light, canned in water, drained solids', 'Finfish and Shellfish Products', 100.0, 90.0, 19.0, 137.0, 176.0, 219.0, 18.0, 22.7, 1.67, 0.66, FALSE, NULL),
('53e73d9e-4654-406b-98ab-2dd13f638d0e', 334536, 'Restaurant, Chinese, fried rice, without meat', 'Restaurant Foods', 100.0, 174.0, 3.84, 44.0, 69.0, 361.0, 11.0, 9.7, 0.46, 0.69, FALSE, NULL),
('d176c137-a3ef-4c60-928d-4ff01ddb1ee3', 334628, 'Restaurant, Latino, tamale, pork', 'Restaurant Foods', 100.0, 174.0, 7.38, 120.0, 152.0, 473.0, 75.0, 26.2, 0.87, 1.2, FALSE, NULL),
('8bc3aab6-ba11-43f3-8944-77538fe9b5eb', 334720, 'Restaurant, Latino, pupusas con frijoles (pupusas, bean)', 'Restaurant Foods', 100.0, 229.0, 5.59, 144.0, 305.0, 305.0, 51.0, 53.6, 1.46, 0.94, FALSE, NULL),
('069ebb77-b957-43a1-82ed-efd7141e039c', 746758, 'Beef, loin, tenderloin roast, separable lean only, boneless, trimmed to 0" fat, select, cooked, roasted', 'Beef Products', 100.0, 176.0, 27.7, 259.0, 352.0, 54.0, 13.0, 13.7, 3.04, 3.7, FALSE, NULL),
('99736ac3-d1d8-4749-8488-6bc5481e461b', 746759, 'Beef, loin, top loin steak, boneless, lip-on, separable lean only, trimmed to 1/8" fat, choice, raw', 'Beef Products', 100.0, 155.0, 22.8, 206.0, 282.0, 45.0, 15.0, 11.3, 1.9, 3.76, FALSE, NULL),
('ec037f7f-18c1-443b-933a-c8a992b0af18', 746760, 'Beef, round, eye of round roast, boneless, separable lean only, trimmed to 0" fat, select, raw', 'Beef Products', 100.0, 122.0, 23.4, 222.0, 312.0, 50.0, 13.0, 11.4, 1.37, 3.38, FALSE, NULL),
('e73b94a9-3bed-44e3-88d7-67bbf40a2fd7', 746761, 'Beef, round, top round roast, boneless, separable lean only, trimmed to 0" fat, select, raw', 'Beef Products', 100.0, 123.0, 23.7, 220.0, 316.0, 55.0, 13.0, 11.9, 2.34, 3.54, FALSE, NULL),
('151c79c6-3b15-4b14-ae70-ab72dae1a2da', 746762, 'Beef, short loin, porterhouse steak, separable lean only, trimmed to 1/8" fat, select, raw', 'Beef Products', 100.0, 145.0, 22.7, 200.0, 266.0, 43.0, 19.0, 10.7, 2.26, 3.52, FALSE, NULL),
('350c5bac-9459-4a3d-bf72-4ed33ab97ba6', 746763, 'Beef, short loin, t-bone steak, bone-in, separable lean only, trimmed to 1/8" fat, choice, cooked, grilled', 'Beef Products', 100.0, 219.0, 27.3, 211.0, 283.0, 67.0, 19.0, 20.0, 3.53, 4.69, FALSE, NULL),
('8548ff7c-6d65-472e-9134-a852be999a2a', 746764, 'Carrots, frozen, unprepared', 'Vegetables and Vegetable Products', 100.0, 37.0, 0.81, 31.0, 210.0, 66.0, 33.0, 10.8, 0.43, 0.28, FALSE, NULL),
('03a34555-444e-4366-af68-277aa1442492', 746765, 'Cheese, dry white, queso seco', 'Dairy and Egg Products', 100.0, 326.0, 24.5, 475.0, 116.0, 1810.0, 661.0, 26.7, 0.18, 3.28, FALSE, NULL),
('7089472f-45a0-476f-be60-a0f8e62b1e6a', 746766, 'Cheese, ricotta, whole milk', 'Dairy and Egg Products', 100.0, 157.0, 7.81, 162.0, 230.0, 105.0, 224.0, 19.7, 0.1, 0.56, FALSE, NULL),
('8834aa8c-060d-43ca-9c8c-efdca2113e65', 746767, 'Cheese, swiss', 'Dairy and Egg Products', 100.0, 393.0, 27.0, 574.0, 71.0, 185.0, 890.0, 33.4, 0.13, 4.37, FALSE, NULL),
('3962bd0f-10ed-4f34-bfbd-fdf2ba6bc440', 746768, 'Figs, dried, uncooked', 'Fruits and Fruit Juices', 100.0, 249.0, 3.3, 67.0, 680.0, 10.0, 162.0, 67.6, 2.03, 0.66, FALSE, NULL),
('aecfdb91-fd73-4dc7-a81c-cb69fe339014', 746769, 'Lettuce, cos or romaine, raw', 'Vegetables and Vegetable Products', 100.0, 17.0, 1.24, 30.0, 253.0, 0.0, 35.0, 13.7, 0.95, 0.25, FALSE, NULL),
('04ec36a7-716e-4499-964c-d8474a7a6615', 746770, 'Melons, cantaloupe, raw', 'Fruits and Fruit Juices', 100.0, 34.0, 0.82, 17.0, 157.0, 30.0, 9.0, 13.0, 0.38, 0.44, FALSE, NULL),
('c8ec5c7d-1866-4f39-877b-66b0a9c24654', 746771, 'Oranges, raw, navels', 'Fruits and Fruit Juices', 100.0, 47.0, 0.91, 23.0, 166.0, 9.0, 43.0, 10.7, 0.33, 0.11, FALSE, NULL),
('2ca805b5-6d8d-48b4-a013-e3d5f1d0d436', 746772, 'Milk, lowfat, fluid, 1% milkfat, with added vitamin A and vitamin D', 'Dairy and Egg Products', 100.0, 43.0, 3.38, 103.0, 159.0, 39.0, 126.0, 12.0, 0.0, 0.43, FALSE, NULL),
('0fd7fed6-4539-45b5-8ea1-7b73a39573ae', 746773, 'Pears, raw, bartlett', 'Fruits and Fruit Juices', 100.0, 57.0, 0.38, 10.0, 87.0, 7.0, 8.0, 5.7, 0.17, 0.07, FALSE, NULL),
('5baf0db2-8c25-41e2-8e8f-d602f19ca8fb', 746774, 'Restaurant, Chinese, sweet and sour pork', 'Restaurant Foods', 100.0, 260.0, 8.88, 133.0, 152.0, 304.0, 46.0, 13.0, 3.07, 1.07, FALSE, NULL),
('59e266fa-1503-4123-83f7-b17f9e358113', 746775, 'Salt, table, iodized', 'Spices and Herbs', 100.0, 0.0, 0.0, 0.0, 2.0, 38700.0, 50.0, 0.0, 0.0, 0.0, FALSE, NULL),
('6a8a51f8-3fed-4f87-bd0c-105d7ec73c6e', 746776, 'Milk, nonfat, fluid, with added vitamin A and vitamin D (fat free or skim)', 'Dairy and Egg Products', 100.0, 34.0, 3.43, 107.0, 167.0, 41.0, 132.0, 12.5, 0.0, 0.45, FALSE, NULL),
('a2a0361a-7ac6-4698-935a-02b187df5f99', 746777, 'Sauce, salsa, ready-to-serve', 'Soups, Sauces, and Gravies', 100.0, 29.0, 1.44, 32.0, 258.0, 656.0, 28.0, 15.2, 0.42, 0.2, FALSE, NULL),
('9d0765c9-27b4-4ba9-95f7-97b7390dbfb1', 746778, 'Milk, reduced fat, fluid, 2% milkfat, with added vitamin A and vitamin D', 'Dairy and Egg Products', 100.0, 50.0, 3.36, 103.0, 159.0, 39.0, 126.0, 12.0, 0.0, 0.43, FALSE, NULL),
('f6eae660-139d-496c-8a57-cf9e1e3a52c3', 746779, 'Sausage, breakfast sausage, beef, pre-cooked, unprepared', 'Sausages and Luncheon Meats', 100.0, 328.0, 13.3, 124.0, 263.0, 866.0, 35.0, 19.7, 1.55, 2.14, FALSE, NULL),
('1566d117-2ece-4d3d-9be5-986a459330df', 746780, 'Sausage, Italian, pork, mild, cooked, pan-fried', 'Sausages and Luncheon Meats', 100.0, 322.0, 18.2, 145.0, 310.0, 766.0, 12.0, 18.6, 1.29, 2.41, FALSE, NULL),
('c4702eb1-d277-4fd5-9d19-4dc7ef071997', 746781, 'Sausage, pork, chorizo, link or ground, cooked, pan-fried', 'Sausages and Luncheon Meats', 100.0, 346.0, 19.3, 207.0, 435.0, 983.0, 37.0, 30.1, 2.34, 2.19, FALSE, NULL),
('f77aeb37-0df0-4429-a53f-d4a90c5cd386', 746782, 'Milk, whole, 3.25% milkfat, with added vitamin D', 'Dairy and Egg Products', 100.0, 60.0, 3.27, 101.0, 150.0, 38.0, 123.0, 11.9, 0.0, 0.42, FALSE, NULL),
('b08e9a10-89de-46df-acec-e386bfc60e12', 746783, 'Sausage, turkey, breakfast links, mild, raw', 'Sausages and Luncheon Meats', 100.0, 169.0, 16.7, 178.0, 310.0, 599.0, 32.0, 18.9, 1.17, 2.58, FALSE, NULL),
('940647f4-2dbd-40b8-af17-d54a23fbc3a3', 746784, 'Sugars, granulated', 'Sweets', 100.0, 385.0, 0.0, 0.0, 2.0, 1.0, 1.0, 0.3, 0.05, 0.01, FALSE, NULL),
('41134f4a-98d0-4100-a2bd-9dc49eabd33e', 746785, 'Turkey, ground, 93% lean, 7% fat, pan-broiled crumbles', 'Poultry Products', 100.0, 220.0, 27.1, 259.0, 304.0, 90.0, 31.0, 29.1, 1.56, 3.77, FALSE, NULL),
('1851cb0d-c7f7-4ba0-90b0-cf08ab363504', 746952, 'Ham, sliced, restaurant', 'Pork Products', 100.0, 121.0, 19.6, 246.0, 484.0, 1030.0, 6.0, 20.3, 0.86, 1.76, FALSE, NULL),
('f361b3c2-eb3a-4359-a14d-15da54584525', 747429, 'Cheese, American, restaurant', 'Dairy and Egg Products', 100.0, 375.0, 17.5, 380.0, 173.0, 1600.0, 508.0, 25.0, 0.24, 2.45, FALSE, NULL),
('d248ee4e-2f53-4bac-9127-5f9fa4a83f66', 747430, 'Beans, Dry, Medium Red (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 25.5, 548.0, 1490.0, 0.0, 193.0, 178.0, 6.27, 3.82, FALSE, NULL),
('0266a336-6573-48c5-b0a6-32c9100f1bd5', 747431, 'Beans, Dry, Red (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 21.3, 380.0, 1540.0, 0.0, 148.0, 152.0, 5.05, 2.84, FALSE, NULL),
('906811a5-61cc-4bf0-b042-eeeed2dda32a', 747432, 'Beans, Dry, Flor de Mayo (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 23.3, 439.0, 1490.0, 0.0, 180.0, 182.0, 4.47, 3.42, FALSE, NULL),
('789c58ab-970b-4ab8-a4ab-c46d368ada6e', 747433, 'Beans, Dry, Brown (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 25.6, 543.0, 1580.0, 0.0, 158.0, 158.0, 4.7, 3.71, FALSE, NULL),
('7b8375a6-84db-469f-9d8e-d3c96edeeb1b', 747434, 'Beans, Dry, Tan (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 26.8, 555.0, 1620.0, 0.0, 178.0, 198.0, 5.85, 3.5, FALSE, NULL),
('1951121a-42df-441e-b42a-493f1d073641', 747435, 'Beans, Dry, Light Tan (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 24.6, 571.0, 1230.0, 0.0, 98.0, 178.0, 7.29, 3.23, FALSE, NULL),
('6f5a85df-e8ca-420f-a41b-c31496fd9fb4', 747436, 'Beans, Dry, Carioca (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 25.2, 535.0, 1390.0, 0.0, 194.0, 179.0, 5.87, 3.36, FALSE, NULL),
('55c236a3-44e1-4abe-8513-17e06c4d7d76', 747437, 'Beans, Dry, Cranberry (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 24.4, 487.0, 1340.0, 0.0, 152.0, 166.0, 5.26, 3.03, FALSE, NULL),
('5b9fc88c-80b4-45cd-8c6e-c8709f475227', 747438, 'Beans, Dry, Light Red Kidney (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 25.0, 549.0, 1540.0, 0.0, 103.0, 158.0, 6.95, 3.5, FALSE, NULL),
('cde0e76a-65fd-4ca3-b07f-e31a137570ac', 747439, 'Beans, Dry, Pink (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 23.4, 491.0, 1490.0, 0.0, 137.0, 164.0, 5.46, 3.26, FALSE, NULL),
('f2afc359-2bd7-47e8-b23a-653c69ab35fc', 747440, 'Beans, Dry, Dark Red Kidney (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 25.9, 612.0, 1490.0, 0.0, 98.0, 164.0, 6.58, 3.29, FALSE, NULL),
('63c9bd13-efc1-40dd-9709-b61cc8d000a7', 747441, 'Beans, Dry, Navy (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 24.1, 523.0, 1470.0, 0.0, 229.0, 180.0, 5.29, 3.31, FALSE, NULL),
('d42e1592-4900-4002-b02b-f8e3b123ddc6', 747442, 'Beans, Dry, Small White (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 24.5, 533.0, 1540.0, 0.0, 236.0, 182.0, 4.93, 3.54, FALSE, NULL),
('6c07faff-e1a9-4cd5-9a3e-53e397e77cdc', 747443, 'Beans, Dry, Small Red (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 23.5, 493.0, 1520.0, 0.0, 149.0, 167.0, 4.95, 3.23, FALSE, NULL),
('c5089f1b-bae2-40ec-9724-aa2cc79dd935', 747444, 'Beans, Dry, Black (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 24.4, 522.0, 1540.0, 0.0, 191.0, 180.0, 5.34, 3.37, FALSE, NULL),
('104e294d-346f-4d0c-809e-a90c0faf14bf', 747445, 'Beans, Dry, Pinto (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 23.7, 507.0, 1510.0, 0.0, 161.0, 170.0, 5.4, 3.43, FALSE, NULL),
('6066ec97-2562-44b7-b00a-d5b54b8befbe', 747446, 'Beans, Dry, Great Northern (0% moisture)', 'Legumes and Legume Products', 100.0, 0.0, 24.7, 519.0, 1520.0, 0.0, 192.0, 176.0, 5.48, 3.45, FALSE, NULL),
('f82aca4f-1f0d-4e97-b871-0707de835f77', 747447, 'Broccoli, raw', 'Vegetables and Vegetable Products', 100.0, 31.0, 2.57, 67.0, 303.0, 36.0, 46.0, 21.0, 0.69, 0.42, FALSE, NULL),
('df2bc2bc-2e23-4c85-a930-898a9eca0475', 747693, 'Ketchup, restaurant', 'Vegetables and Vegetable Products', 100.0, 117.0, 1.11, 25.0, 249.0, 949.0, 14.0, 13.5, 0.43, 0.16, FALSE, NULL),
('52144cdc-83cd-4c06-88eb-9193d8d5dc73', 747997, 'Eggs, Grade A, Large, egg white', 'Dairy and Egg Products', 100.0, 55.0, 10.7, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('72aa775d-54d3-4ec0-9d54-4aa472e55626', 748236, 'Eggs, Grade A, Large, egg yolk', 'Dairy and Egg Products', 100.0, 334.0, 16.2, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('b7b90a8c-d6aa-4895-9ffd-4686be17a3fa', 748278, 'Oil, canola', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('dd0a5c34-9529-4697-9feb-d32834d1a872', 748323, 'Oil, corn', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('3d246e86-3132-4557-b345-06ddde4f4fbe', 748366, 'Oil, soybean', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('a9070444-cf03-4e18-ac69-cc03d5d09d04', 748608, 'Oil, olive, extra virgin', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('0249dbb6-1d4f-49c6-b78e-dd807ba28cf2', 748967, 'Eggs, Grade A, Large, egg whole', 'Dairy and Egg Products', 100.0, 148.0, 12.4, 184.0, 132.0, 129.0, 48.0, 11.4, 1.67, 1.24, FALSE, NULL),
('88e8f7a8-8729-40bb-bc08-930710b0ebde', 749420, 'Pork, cured, bacon, cooked, restaurant', 'Pork Products', 100.0, 500.0, 40.9, 415.0, 557.0, 1830.0, 13.0, 36.5, 1.28, 3.56, FALSE, NULL),
('b8bd7d05-f0e0-490b-aae0-1e3261eddbf7', 789828, 'Butter, stick, unsalted', 'Dairy and Egg Products', 100.0, 0.0, 0.0, 19.0, 19.0, 10.0, 14.0, 1.6, 0.03, 0.07, FALSE, NULL),
('2b79d3a8-25ee-423b-bc8b-047ed79c20f8', 789890, 'Flour, wheat, all-purpose, enriched, bleached', 'Cereal Grains and Pasta', 100.0, 366.0, 10.9, 108.0, 136.0, 2.0, 19.0, 26.7, 5.62, 0.72, FALSE, NULL),
('8de6dc47-2ff9-4e1d-a445-5255a88127ce', 789951, 'Flour, wheat, all-purpose, enriched, unbleached', 'Cereal Grains and Pasta', 100.0, 358.0, 13.1, 115.0, 135.0, 4.0, 21.0, 33.3, 5.41, 0.9, FALSE, NULL),
('eb7ea36b-479d-4e69-a4ba-3f6f65011840', 790018, 'Flour, wheat, all-purpose, unenriched, unbleached', 'Cereal Grains and Pasta', 100.0, 362.0, 12.0, 134.0, 150.0, 2.0, 22.0, 36.1, 1.18, 1.15, FALSE, NULL),
('dfeb2854-f0ab-4bec-b3b0-f2c3775df999', 790085, 'Flour, whole wheat, unenriched', 'Cereal Grains and Pasta', 100.0, 370.0, 15.1, 352.0, 376.0, 3.0, 38.0, 136.0, 3.86, 3.24, FALSE, NULL),
('b2203f15-0f87-45e5-95ae-6581389d3ed5', 790146, 'Flour, bread, white, enriched, unbleached', 'Cereal Grains and Pasta', 100.0, 363.0, 14.3, 124.0, 127.0, 3.0, 19.0, 35.9, 5.55, 1.06, FALSE, NULL),
('9913655d-485f-43c0-aa33-a58067a6c0f8', 790214, 'Flour, rice, white, unenriched', 'Cereal Grains and Pasta', 100.0, 359.0, 6.94, 94.0, 75.0, 5.0, 6.0, 22.9, 0.22, 1.19, FALSE, NULL),
('597b3e6e-1519-4387-a576-7b84aaca2638', 790276, 'Flour, corn, yellow, fine meal, enriched', 'Cereal Grains and Pasta', 100.0, 364.0, 6.2, 92.0, 144.0, 0.0, 0.0, 30.1, 4.44, 0.62, FALSE, NULL),
('cd40fb2b-1ef4-45d5-983b-7fbe2a6617bd', 790508, 'Butter, stick, salted', 'Dairy and Egg Products', 100.0, 0.0, 0.0, 22.0, 23.0, 524.0, 21.0, 1.6, 0.06, 0.07, FALSE, NULL),
('7de0e82e-bcd4-4c0d-a4c4-9600c63a1426', 790577, 'Onions, red, raw', 'Vegetables and Vegetable Products', 100.0, 44.0, 0.94, 41.0, 197.0, 1.0, 17.0, 11.4, 0.24, 0.17, FALSE, NULL),
('c048ec2c-0f55-4d58-9c79-ca1f1b631ff3', 790646, 'Onions, yellow, raw', 'Vegetables and Vegetable Products', 100.0, 38.0, 0.83, 34.0, 182.0, 1.0, 15.0, 9.0, 0.28, 0.2, FALSE, NULL),
('c4e9a4fa-d6a8-4006-a1aa-e1c9c471baea', 1104647, 'Garlic, raw', 'Vegetables and Vegetable Products', 100.0, 143.0, 6.62, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('a0d6ca11-a403-4474-8b1a-e1f494d340ff', 1104705, 'Flour, soy, defatted', 'Legumes and Legume Products', 100.0, 366.0, 51.1, 726.0, 2480.0, 2.0, 338.0, 313.0, 7.34, 4.44, FALSE, NULL),
('477f02da-9552-4128-950f-3bf42a9c3dd2', 1104766, 'Flour, soy, full-fat', 'Legumes and Legume Products', 100.0, 452.0, 38.6, 607.0, 1860.0, 2.0, 258.0, 254.0, 9.51, 3.75, FALSE, NULL),
('0f3da064-6540-40d2-8278-6c982d5fa5ff', 1104812, 'Flour, rice, brown', 'Cereal Grains and Pasta', 100.0, 365.0, 7.19, 319.0, 265.0, 1.0, 10.0, 124.0, 1.5, 1.91, FALSE, NULL),
('a7c2c24c-ce69-4042-a7b7-d51766508844', 1104867, 'Flour, rice, glutinous', 'Cereal Grains and Pasta', 100.0, 358.0, 6.69, 63.0, 80.0, 6.0, 10.0, 21.2, 0.34, 1.34, FALSE, NULL),
('bb92bd28-722f-4f6d-889a-bb63e012ca9d', 1104913, 'Flour, pastry, unenriched, unbleached', 'Cereal Grains and Pasta', 100.0, 358.0, 8.75, 102.0, 142.0, 1.0, 17.0, 22.4, 0.87, 0.74, FALSE, NULL),
('43f2668f-9145-4fad-b221-5d97236f03a2', 1104962, 'Onions, white, raw', 'Vegetables and Vegetable Products', 100.0, 35.0, 0.89, 29.0, 141.0, 2.0, 21.0, 9.3, 0.15, 0.12, FALSE, NULL),
('e7232ac7-344a-495d-b71f-04c4b19f9d74', 1105073, 'Bananas, overripe, raw', 'Fruits and Fruit Juices', 100.0, 85.0, 0.73, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('6d67f8dd-8b21-4781-8a80-eba203866b1d', 1105314, 'Bananas, ripe and slightly ripe, raw', 'Fruits and Fruit Juices', 100.0, 97.0, 0.74, 22.0, 326.0, 0.0, 5.0, 28.0, 0.0, 0.16, FALSE, NULL),
('1eda1b7e-31f5-43d5-b56a-96c7fde3c136', 1750339, 'Apples, red delicious, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.188, 9.18, 95.3, 0.0, 4.66, 4.7, 0.0, 0.02, FALSE, NULL),
('2a8d4e8d-4c08-4a90-9de4-c60741c1b8f7', 1750340, 'Apples, fuji, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.148, 9.78, 104.0, 1.01, 5.98, 4.67, 0.015, 0.017, FALSE, NULL),
('4774bc26-f470-48ab-aa64-e9e6ea8bfdc2', 1750341, 'Apples, gala, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.133, 8.44, 106.0, 0.279, 6.64, 4.95, 0.06, 0.024, FALSE, NULL),
('221f0127-5212-4b47-9e88-ab8349470385', 1750342, 'Apples, granny smith, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.266, 9.86, 116.0, 0.0, 5.48, 5.06, 0.07, 0.023, FALSE, NULL),
('d276bf9d-a4a1-4887-870f-0cfe1556f717', 1750343, 'Apples, honeycrisp, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.102, 7.85, 98.0, 0.138, 3.87, 4.15, 0.0, 0.018, FALSE, NULL),
('817f1c7f-08ac-45ed-a811-08c5e8d2b447', 1750348, 'Oil, peanut', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('954e7990-91d0-4f5b-a85b-2639da1120be', 1750349, 'Oil, sunflower', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('dc9eec67-5e45-451f-ba0c-4ed0b98dfc8f', 1750350, 'Oil, safflower', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('e8ec4be9-a95c-4bdb-9244-ce9958911bd2', 1750351, 'Oil, olive, extra light', 'Fats and Oils', 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, FALSE, NULL),
('0d3c0dde-2917-4d7d-90a0-0dd0c7a09672', 1999626, 'Mushroom, lion''s mane', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.5, 94.0, 443.0, 0.336, 0.0, 11.7, 0.688, 0.745, FALSE, NULL),
('9066f3b4-b0a3-40ad-83f6-2b2d9691ab1d', 1999627, 'Mushroom, oyster', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.9, 86.2, 282.0, 1.13, 0.0, 13.9, 0.698, 0.684, FALSE, NULL),
('1b33988b-2f30-45cb-aa15-7e13d048db87', 1999628, 'Mushrooms, shiitake', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.41, 75.8, 243.0, 0.936, 0.766, 14.1, 0.144, 0.764, FALSE, NULL),
('7192df84-a869-412c-853b-8345b46ca13f', 1999629, 'Mushrooms, white button', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.89, 93.0, 373.0, 6.44, 5.46, 10.2, 0.227, 0.511, FALSE, NULL),
('81e1b0b4-998f-4951-bc6c-afa79349e704', 1999630, 'Soy milk, unsweetened, plain, shelf stable', 'Legumes and Legume Products', 100.0, 0.0, 3.55, 68.6, 158.0, 34.3, 101.0, 21.5, 0.542, 0.312, FALSE, NULL),
('ec1cc6ca-a28a-454e-9033-599cf76b1beb', 1999631, 'Almond milk, unsweetened, plain, shelf stable', 'Beverages', 100.0, 0.0, 0.555, 29.8, 30.8, 59.6, 173.0, 6.78, 0.29, 0.172, FALSE, NULL),
('991ac270-1540-4f11-ae52-550cdad1d387', 1999632, 'Spinach, baby', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.85, 39.0, 582.0, 111.0, 68.4, 92.9, 1.26, 0.447, FALSE, NULL),
('39291176-6427-4f57-aae4-de0a9a6e0fc7', 1999633, 'Spinach, mature', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.91, 40.6, 460.0, 107.0, 66.6, 93.0, 1.05, 0.423, FALSE, NULL),
('ac00fcdd-adcf-4871-876e-a08ab62f0812', 1999634, 'Tomato, roma', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.696, 19.1, 193.0, 0.0, 9.96, 8.09, 0.103, 0.082, FALSE, NULL),
('5d4a5073-539c-46a2-8fc9-6153d12b7b6c', 2003586, 'Flour, 00', 'Cereal Grains and Pasta', 100.0, 0.0, 11.4, 106.0, 136.0, 0.0, 18.7, 25.8, 0.986, 0.791, FALSE, NULL),
('f2ba218f-7bf7-4a93-a7a5-a6dad2fa462a', 2003587, 'Flour, spelt, whole grain', 'Cereal Grains and Pasta', 100.0, 0.0, 14.5, 362.0, 350.0, 0.0, 30.0, 124.0, 3.77, 3.59, FALSE, NULL),
('a2d236f5-314a-427b-a819-0ee08a5daa63', 2003588, 'Flour, semolina, coarse and semi-coarse', 'Cereal Grains and Pasta', 100.0, 0.0, 11.7, 130.0, 174.0, 0.0, 17.2, 35.2, 1.55, 0.947, FALSE, NULL),
('1a23baf8-f825-4631-880f-dc34535cb957', 2003589, 'Flour, semolina, fine', 'Cereal Grains and Pasta', 100.0, 0.0, 13.3, 163.0, 208.0, 0.426, 19.8, 46.3, 2.43, 1.24, FALSE, NULL),
('895ed6bf-5c5c-42ce-9bd7-e683903945c7', 2003590, 'Apple juice, with added vitamin C, from concentrate, shelf stable', 'Fruits and Fruit Juices', 100.0, 0.0, 0.086, 5.99, 95.9, 4.87, 7.1, 4.86, 0.04, 0.002, FALSE, NULL),
('e05d1da0-c6d3-4033-bd92-0bc9915d1617', 2003591, 'Orange juice, no pulp, not fortified, from concentrate, refrigerated', 'Fruits and Fruit Juices', 100.0, 0.0, 0.734, 18.2, 180.0, 5.23, 12.8, 10.6, 0.065, 0.032, FALSE, NULL),
('06afb5ce-7cf0-4d6f-9ac0-e64e99b63759', 2003592, 'Grape juice, purple, with added vitamin C, from concentrate, shelf stable', 'Fruits and Fruit Juices', 100.0, 0.0, 0.258, 12.0, 49.8, 4.02, 9.74, 7.87, 0.041, 0.057, FALSE, NULL),
('1870bb9c-d54d-4ae8-b790-933c8e9a1c54', 2003593, 'Grape juice, white, with added vitamin C, from concentrate, shelf stable', 'Fruits and Fruit Juices', 100.0, 0.0, 0.094, 12.6, 49.0, 7.19, 7.32, 4.27, 0.119, 0.018, FALSE, NULL),
('799ccc2e-0c85-4b6c-8db0-904e41ba297d', 2003594, 'Cranberry juice, not fortified, from concentrate, shelf stable', 'Fruits and Fruit Juices', 100.0, 0.0, 0.0, 3.3, 70.6, 6.34, 6.64, 4.38, 0.126, 0.041, FALSE, NULL),
('cc4af145-dae0-4380-86f8-04b527d0c524', 2003595, 'Grapefruit juice, red, not fortified, not from concentrate, refrigerated', 'Fruits and Fruit Juices', 100.0, 0.0, 0.57, 14.6, 128.0, 1.03, 8.88, 7.75, 0.015, 0.027, FALSE, NULL),
('f712ab1e-a193-4c66-bbd7-80f155ab95b1', 2003596, 'Tomato juice, with added ingredients, from concentrate, shelf stable', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.859, 18.5, 198.0, 236.0, 9.74, 10.4, 0.299, 0.104, FALSE, NULL),
('258f69fa-9dc0-4271-b1fe-e98fa2091f22', 2003597, 'Orange juice, no pulp, not fortified, not from concentrate, refrigerated', 'Fruits and Fruit Juices', 100.0, 0.0, 0.812, 19.0, 183.0, 0.075, 9.29, 10.3, 0.071, 0.032, FALSE, NULL),
('fd88222a-f78e-4026-9c35-a0191541a764', 2003598, 'Mushroom, portabella', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.75, 87.5, 349.0, 5.31, 3.24, 9.04, 0.139, 0.389, FALSE, NULL),
('68250641-44d6-4312-80b3-ca7f26b0b8da', 2003599, 'Mushroom, king oyster', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.41, 89.9, 294.0, 0.771, 0.0, 13.5, 0.338, 0.625, FALSE, NULL),
('3fbd8d7c-6ebd-424f-b3c0-8dfc0c956af9', 2003600, 'Mushroom, enoki', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.42, 84.1, 402.0, 0.417, 1.43, 12.8, 1.28, 0.482, FALSE, NULL),
('4e56c997-f6d0-4122-aeb0-c40f326b5efd', 2003601, 'Mushroom, crimini', 'Vegetables and Vegetable Products', 100.0, 0.0, 3.09, 100.0, 380.0, 4.57, 4.1, 10.2, 0.312, 0.512, FALSE, NULL),
('808485f5-49b5-4c56-9619-3788de279d5b', 2003602, 'Mushroom, maitake', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.2, 72.4, 260.0, 0.345, 0.0, 11.0, 0.209, 0.763, FALSE, NULL),
('7ceaf18c-0f0c-445a-bcc0-0023365c0151', 2003603, 'Mushroom, beech', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.18, 85.9, 376.0, 0.891, 0.432, 10.5, 0.739, 0.498, FALSE, NULL),
('a0ca5d1e-c7de-4893-9ec4-5d0dc32a8aa8', 2003604, 'Mushroom, pioppini', 'Vegetables and Vegetable Products', 100.0, 0.0, 3.5, 121.0, 392.0, 0.0, 0.0, 16.0, 0.486, 0.857, FALSE, NULL),
('9533be72-c5f2-4305-a4f6-1abb63b31920', 2257044, 'Soy milk, sweetened, plain, refrigerated', 'Legumes and Legume Products', 100.0, 0.0, 2.78, 46.0, 118.0, 39.4, 155.0, 17.5, 0.367, 0.264, FALSE, NULL),
('ca057e13-d19b-4299-8c6a-8faa128ce23a', 2257045, 'Almond milk, unsweetened, plain, refrigerated', 'Beverages', 100.0, 0.0, 0.656, 19.1, 49.3, 59.2, 158.0, 8.25, 0.125, 0.077, FALSE, NULL),
('01acfdaf-2798-44ae-a98c-3ce306bc5a00', 2257046, 'Oat milk, unsweetened, plain, refrigerated', 'Beverages', 100.0, 0.0, 0.797, 89.2, 148.0, 42.0, 148.0, 5.91, 0.261, 0.091, FALSE, NULL),
('44946c86-7f55-4ff7-93f9-f0c84fcb8105', 2258586, 'Carrots, mature, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.941, 39.8, 280.0, 86.6, 30.5, 12.4, 0.15, 0.236, FALSE, NULL),
('b4aaab90-235b-4393-8c5c-c7556b4d6c0e', 2258587, 'Carrots, baby, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.805, 29.6, 237.0, 62.7, 42.2, 11.1, 0.088, 0.16, FALSE, NULL),
('ce7cd6a6-8b41-4155-ac28-a0aee2b646ad', 2258588, 'Peppers, bell, green, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.715, 16.8, 163.0, 0.0, 7.5, 9.0, 0.186, 0.126, FALSE, NULL),
('45e83e57-5d4a-4b63-9cc1-94f736c72e93', 2258589, 'Peppers, bell, yellow, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.819, 24.8, 197.0, 0.0, 6.69, 10.3, 0.36, 0.193, FALSE, NULL),
('c747f298-10b2-4bf8-b324-424338186df6', 2258590, 'Peppers, bell, red, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.896, 26.6, 213.0, 0.0, 6.36, 11.0, 0.353, 0.202, FALSE, NULL),
('8243907b-79df-42ba-9c31-619f05bb25c6', 2258591, 'Peppers, bell, orange, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.882, 26.6, 201.0, 0.0, 4.9, 10.4, 0.365, 0.242, FALSE, NULL),
('4fcc92a5-ac29-4e13-a9ce-20e02962e204', 2259792, 'Buttermilk, low fat', 'Dairy and Egg Products', 100.0, 0.0, 3.46, 92.2, 158.0, 92.5, 120.0, 11.0, 0.013, 0.426, FALSE, NULL),
('8b04df5f-a6de-4026-b433-316c58ce049f', 2259793, 'Yogurt, plain, whole milk', 'Dairy and Egg Products', 100.0, 0.0, 3.82, 101.0, 164.0, 41.8, 127.0, 11.4, 0.0, 0.428, FALSE, NULL),
('5e1cb88d-30b1-4c46-a39e-42d66a008d17', 2259794, 'Yogurt, Greek, plain, whole milk', 'Dairy and Egg Products', 100.0, 0.0, 8.78, 126.0, 147.0, 33.8, 111.0, 10.7, 0.0, 0.473, FALSE, NULL),
('afbf618e-cfac-4477-8f93-027285d82725', 2259795, 'Cheese, parmesan, grated, refrigerated', 'Dairy and Egg Products', 100.0, 0.0, 30.1, 625.0, 74.0, 1050.0, 950.0, 35.4, 0.14, 4.62, FALSE, NULL),
('8851cb46-9876-45e8-95d7-a508e6ca4e55', 2259796, 'Cheese, feta, whole milk, crumbled', 'Dairy and Egg Products', 100.0, 0.0, 19.7, 328.0, 105.0, 1030.0, 371.0, 17.7, 0.068, 2.35, FALSE, NULL),
('b9d102f3-45aa-4fae-8575-66195e52b389', 2261420, 'Flour, almond', 'Nut and Seed Products', 100.0, 0.0, 26.2, 512.0, 667.0, 0.894, 232.0, 251.0, 3.22, 2.8, FALSE, NULL),
('fbb999bc-9eb7-4571-b101-93e8fb10cfe2', 2261421, 'Flour, oat, whole grain', 'Cereal Grains and Pasta', 100.0, 0.0, 13.2, 372.0, 373.0, 3.62, 42.8, 125.0, 4.0, 2.75, FALSE, NULL),
('8f084c85-0366-4b33-ae96-50596c4845fb', 2261422, 'Flour, potato', 'Vegetables and Vegetable Products', 100.0, 0.0, 8.11, 197.0, 1270.0, 47.7, 44.1, 73.6, 12.0, 0.9, FALSE, NULL),
('7825e07d-4772-4610-8082-17b3c4cd494f', 2262072, 'Peanut butter, creamy', 'Legumes and Legume Products', 100.0, 0.0, 24.0, 393.0, 654.0, 221.0, 49.8, 193.0, 1.85, 3.06, FALSE, NULL),
('c7a606bb-7e6a-480f-8930-d1f773efa4b1', 2262073, 'Sesame butter, creamy', 'Nut and Seed Products', 100.0, 0.0, 19.7, 719.0, 408.0, 63.6, 116.0, 357.0, 7.0, 6.04, FALSE, NULL),
('eb3f8b28-e581-4c9d-bf56-8420df241daf', 2262074, 'Almond butter, creamy', 'Nut and Seed Products', 100.0, 0.0, 20.8, 507.0, 745.0, 0.996, 264.0, 268.0, 4.11, 3.18, FALSE, NULL),
('7482ae17-5953-45b7-a84b-398bce7a7686', 2262075, 'Flaxseed, ground', 'Nut and Seed Products', 100.0, 0.0, 18.0, 556.0, 793.0, 36.7, 230.0, 372.0, 5.78, 4.74, FALSE, NULL),
('b9d214ed-fbc3-417c-bc6a-1551e72f9977', 2346384, 'Cottage cheese, full fat, large or small curd', 'Dairy and Egg Products', 100.0, 0.0, 11.6, 154.0, 124.0, 350.0, 88.3, 9.19, 0.0, 0.454, FALSE, NULL),
('5889ce07-b4b8-4bff-8cc7-07118c5742d1', 2346385, 'Cream cheese, full fat, block', 'Dairy and Egg Products', 100.0, 0.0, 5.79, 108.0, 125.0, 368.0, 97.1, 8.91, 0.0, 0.589, FALSE, NULL),
('978bf4b7-91e2-48b1-9ca9-980b502b7160', 2346386, 'Cream, heavy', 'Dairy and Egg Products', 100.0, 0.0, 2.02, 57.0, 96.9, 20.6, 61.2, 5.97, 0.0, 0.223, FALSE, NULL),
('3caeb871-ecf0-4b2c-a52b-83f18e22a25f', 2346387, 'Cream, sour, full fat', 'Dairy and Egg Products', 100.0, 0.0, 3.07, 102.0, 154.0, 50.0, 107.0, 10.1, 0.0, 0.392, FALSE, NULL),
('33f4f503-ec53-4158-a403-0f9c650dd42a', 2346388, 'Lettuce, iceberg, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.742, 19.2, 139.0, 16.1, 14.2, 6.31, 0.033, 0.173, FALSE, NULL),
('f2180848-fae5-4f6a-b42d-2906ab64a8be', 2346389, 'Lettuce, romaine, green, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.977, 23.4, 260.0, 23.0, 27.6, 12.0, 0.267, 0.252, FALSE, NULL),
('13624c6c-1527-48bc-9e0f-4c22b106d0e0', 2346390, 'Lettuce, leaf, red, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.883, 24.4, 321.0, 24.9, 42.6, 12.4, 0.402, 0.303, FALSE, NULL),
('4fa5e262-56f5-4b97-9a5f-935668114a0c', 2346391, 'Lettuce, leaf, green, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.09, 26.6, 277.0, 28.9, 39.8, 12.8, 0.32, 0.31, FALSE, NULL),
('da220231-8c0a-4315-9b63-4f827b23a1f3', 2346392, 'Nuts, pine nuts, raw', 'Nut and Seed Products', 100.0, 0.0, 15.7, 540.0, 655.0, 0.0, 8.74, 206.0, 5.36, 5.71, FALSE, NULL),
('134a91db-7e62-450b-8151-c74d3613e8fb', 2346393, 'Nuts, almonds, whole, raw', 'Nut and Seed Products', 100.0, 0.0, 21.5, 503.0, 733.0, 0.0, 254.0, 258.0, 3.74, 2.86, FALSE, NULL),
('14348c16-30d1-4c83-8c21-71fdd434c142', 2346394, 'Nuts, walnuts, English, halves, raw', 'Nut and Seed Products', 100.0, 0.0, 14.6, 365.0, 424.0, 0.0, 88.3, 142.0, 2.24, 2.76, FALSE, NULL),
('96c298da-fb3a-4da0-b364-0929e3c8c05a', 2346395, 'Nuts, pecans, halves, raw', 'Nut and Seed Products', 100.0, 0.0, 9.96, 253.0, 360.0, 0.0, 54.8, 103.0, 2.37, 3.93, FALSE, NULL),
('0116eaf1-7b65-4ef7-b22d-afb778e1aeaa', 2346396, 'Oats, whole grain, rolled, old fashioned', 'Cereal Grains and Pasta', 100.0, 0.0, 13.5, 387.0, 350.0, 0.668, 45.5, 126.0, 4.34, 2.74, FALSE, NULL),
('cb665086-eb83-49e2-99be-ac2bd7443e65', 2346397, 'Oats, whole grain, steel cut', 'Cereal Grains and Pasta', 100.0, 0.0, 12.5, 417.0, 376.0, 0.311, 51.3, 129.0, 3.8, 2.84, FALSE, NULL),
('c465be7f-6ae9-4691-aa30-daf8df951512', 2346398, 'Pineapple, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.461, 5.42, 137.0, 0.0, 12.5, 13.4, 0.054, 0.107, FALSE, NULL),
('b367915d-de7e-4d44-ae90-6d3fb25c3895', 2346399, 'Cherries, sweet, dark red, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.04, 23.2, 230.0, 0.0, 12.3, 12.1, 0.105, 0.065, FALSE, NULL),
('5c348abe-7ebd-402a-9449-f8e46a0eac17', 2346400, 'Beans, snap, green, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.97, 41.2, 290.0, 0.0, 40.0, 28.2, 0.652, 0.35, FALSE, NULL),
('cda1f44d-55a4-432b-840d-f0fedac9eaf6', 2346401, 'Potatoes, russet, without skin, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.27, 55.2, 450.0, 2.74, 7.8, 25.6, 0.384, 0.378, FALSE, NULL),
('ad237d9f-a9b9-43ab-a2aa-26c3196445c2', 2346402, 'Potatoes, red, without skin, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.06, 55.6, 472.0, 2.86, 5.13, 23.6, 0.391, 0.392, FALSE, NULL),
('381eccf8-ab3c-4663-8191-d669649162ef', 2346403, 'Potatoes, gold, without skin, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.81, 57.0, 446.0, 2.24, 5.94, 22.3, 0.373, 0.374, FALSE, NULL),
('e265b908-0788-4d93-8734-944a004c44f8', 2346404, 'Sweet potatoes, orange flesh, without skin, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.58, 36.7, 486.0, 0.0, 22.3, 19.1, 0.398, 0.337, FALSE, NULL),
('8c9dfbdc-f795-478d-aeb2-3e255bd9458d', 2346405, 'Celery, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.492, 21.6, 265.0, 97.2, 46.3, 10.9, 0.0, 0.093, FALSE, NULL),
('962d5945-8ef4-4af8-a55b-2a495ee029be', 2346406, 'Cucumber, with peel, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.625, 23.2, 170.0, 1.52, 16.3, 10.1, 0.0, 0.204, FALSE, NULL),
('c07a2527-a263-4ec3-b8f3-cdc00eef4624', 2346407, 'Cabbage, green, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.961, 26.9, 207.0, 16.1, 41.8, 13.9, 0.066, 0.211, FALSE, NULL),
('ac731a0f-0419-4d2e-8ffb-bbd0c483f764', 2346408, 'Cabbage, red, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.24, 35.8, 269.0, 11.7, 31.0, 13.3, 0.0, 0.245, FALSE, NULL),
('994433da-1e94-4fee-adaf-d8292388d885', 2346409, 'Strawberries, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.641, 23.0, 161.0, 0.0, 16.9, 12.5, 0.255, 0.114, FALSE, NULL),
('5e77c434-bc1a-4a69-abd0-36544b39e823', 2346410, 'Raspberries, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.01, 27.5, 156.0, 0.0, 16.4, 19.2, 0.45, 0.221, FALSE, NULL),
('e53e2334-0e0d-46c0-9514-dfa3f744958e', 2346411, 'Blueberries, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.703, 13.0, 85.6, 0.0, 11.7, 6.18, 0.34, 0.085, FALSE, NULL),
('ccb4eed9-cc05-477a-a656-263cff0033bc', 2346412, 'Grapes, red, seedless, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.914, 24.9, 229.0, 7.0, 10.2, 8.56, 0.162, 0.036, FALSE, NULL),
('a0ba429b-ec5b-4266-b61c-b5275207cfcf', 2346413, 'Grapes, green, seedless, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.899, 22.0, 218.0, 3.17, 9.9, 7.12, 0.199, 0.027, FALSE, NULL),
('57ef41d7-1c58-4043-a45d-d545bbb6af58', 2346414, 'Applesauce, unsweetened, with added vitamin C', 'Fruits and Fruit Juices', 100.0, 0.0, 0.273, 9.12, 108.0, 0.504, 3.82, 3.94, 0.045, 0.003, FALSE, NULL),
('5a0e11aa-90a8-4235-8493-b268f0dfb54f', 2512371, 'Flour, amaranth', 'Cereal Grains and Pasta', 100.0, 0.0, 13.2, 425.0, 396.0, 0.0, 135.0, 233.0, 7.56, 3.0, FALSE, NULL),
('bdda4ef8-086c-44a3-a85e-655479313b36', 2512372, 'Flour, quinoa', 'Cereal Grains and Pasta', 100.0, 0.0, 11.9, 369.0, 551.0, 6.32, 37.6, 164.0, 4.53, 2.77, FALSE, NULL),
('099b2be3-2071-4d33-92a3-e169b236751c', 2512373, 'Flour, sorghum', 'Cereal Grains and Pasta', 100.0, 0.0, 8.27, 262.0, 335.0, 0.0, 11.4, 116.0, 3.72, 1.66, FALSE, NULL),
('d55f35a2-cc1b-42ea-92a8-b79d0b5bb010', 2512374, 'Flour, buckwheat', 'Cereal Grains and Pasta', 100.0, 0.0, 8.88, 282.0, 378.0, 0.0, 30.6, 167.0, 3.84, 1.76, FALSE, NULL),
('4e4f7595-1ba8-4b44-9b03-dcaeea75783c', 2512375, 'Flour, rye', 'Cereal Grains and Pasta', 100.0, 0.0, 8.4, 280.0, 434.0, 0.0, 32.2, 95.4, 2.54, 2.33, FALSE, NULL),
('65e94706-ce59-4b0f-b812-23626a66b165', 2512376, 'Flour, barley', 'Cereal Grains and Pasta', 100.0, 0.0, 8.72, 234.0, 367.0, 20.3, 35.6, 88.0, 3.3, 2.14, FALSE, NULL),
('c24b23fc-3fab-4f7c-9ce6-1226d79bcf8f', 2512377, 'Flour, cassava', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.918, 39.2, 198.0, 13.1, 74.6, 33.6, 3.99, 0.41, FALSE, NULL),
('150169f9-209e-4ffd-b625-51c54c89d3c8', 2512378, 'Buckwheat, whole grain', 'Cereal Grains and Pasta', 100.0, 0.0, 11.1, 374.0, 414.0, 0.0, 13.6, 203.0, 2.44, 2.24, FALSE, NULL),
('7fa7f9d6-aba5-464d-8c4b-4f00d6f74b3b', 2512379, 'Millet, whole grain', 'Cereal Grains and Pasta', 100.0, 0.0, 10.0, 258.0, 214.0, 0.0, 9.1, 106.0, 2.53, 2.26, FALSE, NULL),
('1d000fea-31d7-4a12-8e6d-26045413b926', 2512380, 'Rice, brown, long grain, unenriched, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 7.25, 303.0, 250.0, 0.0, 8.06, 115.0, 1.24, 1.85, FALSE, NULL),
('1ad01d7b-c8f5-4ea1-9b10-fa7eccfb5a22', 2512381, 'Rice, white, long grain, unenriched, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 7.04, 108.0, 82.3, 0.462, 4.46, 26.5, 0.14, 1.35, FALSE, NULL),
('fa7df977-04f5-4b4c-8e1b-8117e2c6474c', 2514743, 'Beef, ground, 90% lean meat / 10% fat, raw', 'Beef Products', 100.0, 0.0, 18.2, 148.0, 281.0, 61.6, 7.14, 16.5, 2.13, 4.19, FALSE, NULL),
('298b0857-7e45-4c84-b3a7-8c3a471ab846', 2514744, 'Beef, ground, 80% lean meat / 20% fat, raw', 'Beef Products', 100.0, 0.0, 17.5, 144.0, 273.0, 54.9, 6.89, 16.4, 1.96, 3.85, FALSE, NULL),
('1dce7ea4-5ff0-4d98-9ad2-d17e88568df8', 2514745, 'Pork, ground, raw', 'Pork Products', 100.0, 0.0, 17.8, 173.0, 318.0, 53.6, 5.86, 19.0, 0.788, 2.23, FALSE, NULL),
('22c8bb88-006b-4d55-a4a3-b8ef552412a7', 2514746, 'Chicken, ground, with additives, raw', 'Poultry Products', 100.0, 0.0, 17.9, 166.0, 302.0, 63.0, 5.82, 20.5, 0.593, 1.18, FALSE, NULL),
('7c623026-507c-4d69-8d37-d13676b9b7d1', 2514747, 'Turkey, ground, 93% lean/ 7% fat, raw', 'Poultry Products', 100.0, 0.0, 17.3, 162.0, 246.0, 80.2, 23.6, 17.3, 1.09, 2.95, FALSE, NULL),
('a7d146be-756b-40d7-8127-b4bc166e4dfd', 2515373, 'Nuts, brazilnuts, raw', 'Nut and Seed Products', 100.0, 0.0, 15.0, 707.0, 592.0, 0.0, 168.0, 351.0, 2.47, 3.82, FALSE, NULL),
('58fb5237-eb80-42aa-9439-5026e16233d9', 2515374, 'Nuts, cashew nuts, raw', 'Nut and Seed Products', 100.0, 0.0, 17.4, 532.0, 638.0, 4.76, 42.0, 251.0, 5.99, 5.07, FALSE, NULL),
('c16bcb67-b579-42b2-a4b5-c05fe157cefa', 2515375, 'Nuts, hazelnuts or filberts, raw', 'Nut and Seed Products', 100.0, 0.0, 13.5, 321.0, 636.0, 0.0, 135.0, 156.0, 3.46, 2.33, FALSE, NULL),
('fdea30bf-cdf4-40cd-b695-a22905a64569', 2515376, 'Peanuts, raw', 'Legumes and Legume Products', 100.0, 0.0, 23.2, 380.0, 636.0, 1.49, 49.1, 180.0, 1.55, 2.78, FALSE, NULL),
('e477ae48-4779-4b1a-bcc6-b5ac8418e0b0', 2515377, 'Flour, chestnut', 'Nut and Seed Products', 100.0, 0.0, 5.29, 134.0, 1030.0, 0.0, 55.8, 68.8, 1.62, 1.04, FALSE, NULL),
('b73e358b-472d-4763-a650-fdfc66a679c9', 2515378, 'Nuts, macadamia nuts, raw', 'Nut and Seed Products', 100.0, 0.0, 7.79, 208.0, 373.0, 0.0, 52.7, 107.0, 1.88, 1.2, FALSE, NULL),
('95652df6-0d7a-4317-896a-5247bc308280', 2515379, 'Nuts, pistachio nuts, raw', 'Nut and Seed Products', 100.0, 0.0, 20.5, 500.0, 947.0, 0.0, 117.0, 110.0, 3.46, 2.18, FALSE, NULL),
('b7bd37bd-4c54-4729-9a65-d8ed881c7a78', 2515380, 'Seeds, pumpkin seeds (pepitas), raw', 'Nut and Seed Products', 100.0, 0.0, 29.9, 1150.0, 691.0, 0.0, 37.4, 500.0, 8.36, 6.34, FALSE, NULL),
('41cb1d20-9899-4ab8-84ef-2ca1a8e9a579', 2515381, 'Seeds, sunflower seed, kernel, raw', 'Nut and Seed Products', 100.0, 0.0, 18.9, 732.0, 657.0, 0.0, 116.0, 302.0, 4.37, 5.58, FALSE, NULL),
('67c43c6f-e4ec-4b7b-8830-9524e7b2b2c5', 2515382, 'Flour, coconut', 'Nut and Seed Products', 100.0, 0.0, 16.1, 507.0, 2090.0, 46.9, 36.0, 248.0, 8.02, 3.7, FALSE, NULL),
('42da2d27-c405-4fdc-b848-79f9d5b330d3', 2644281, 'Beans, cannellini, dry', 'Legumes and Legume Products', 100.0, 0.0, 21.6, 412.0, 1420.0, 0.0, 143.0, 154.0, 6.67, 2.72, FALSE, NULL),
('7dc9bfb7-bc2c-4a35-ac51-4225f38237d8', 2644282, 'Chickpeas, (garbanzo beans, bengal gram), dry', 'Legumes and Legume Products', 100.0, 0.0, 21.3, 353.0, 1070.0, 8.78, 111.0, 135.0, 5.09, 3.12, FALSE, NULL),
('d0d44605-bd16-42a9-af6e-182fa87b73c8', 2644283, 'Lentils, dry', 'Legumes and Legume Products', 100.0, 0.0, 23.6, 374.0, 949.0, 0.0, 61.8, 107.0, 7.16, 3.86, FALSE, NULL),
('e81ceeae-c530-478f-acbd-54adfa01b23c', 2644284, 'Blackeye pea, dry', 'Legumes and Legume Products', 100.0, 0.0, 21.2, 428.0, 1240.0, 3.46, 71.4, 184.0, 5.93, 3.65, FALSE, NULL),
('f5d2ccec-7398-47d7-b3c6-7ff6f720fde2', 2644285, 'Beans, black, canned, sodium added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 6.91, 91.0, 253.0, 218.0, 43.3, 32.5, 1.69, 0.74, FALSE, NULL),
('ae1e247c-79bd-4a34-8219-e0cfdb7a0938', 2644286, 'Beans, navy, canned, sodium added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 6.57, 86.7, 184.0, 190.0, 63.9, 29.6, 1.6, 0.631, FALSE, NULL),
('619c2195-618d-4e5e-a399-82de37c31bde', 2644287, 'Beans, cannellini, canned, sodium added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 7.41, 94.7, 203.0, 164.0, 69.2, 29.3, 1.4, 0.516, FALSE, NULL),
('11a7f94a-8fc7-4e3c-8293-39d1ca69e1c4', 2644288, 'Chickpeas (garbanzo beans, bengal gram), canned, sodium added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 7.02, 86.5, 137.0, 202.0, 40.5, 28.4, 1.04, 0.724, FALSE, NULL),
('894bda14-c741-4882-8d4b-e6f92a7e7f98', 2644289, 'Beans, kidney, dark red, canned, sodium added, sugar added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 7.8, 104.0, 227.0, 172.0, 56.9, 29.3, 1.44, 0.56, FALSE, NULL),
('f8bde1f1-3bec-43ed-a2eb-e97690a701b2', 2644290, 'Beans, kidney, light red, canned, sodium added, sugar added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 7.31, 93.6, 209.0, 158.0, 60.2, 25.5, 1.3, 0.526, FALSE, NULL),
('51b3bb9b-823d-4a53-806a-76a51e167730', 2644291, 'Peas, green, sweet, canned, sodium added, sugar added, drained and rinsed', 'Vegetables and Vegetable Products', 100.0, 0.0, 4.73, 77.7, 109.0, 207.0, 28.2, 21.9, 1.14, 0.712, FALSE, NULL),
('150a0326-96d1-433d-85be-39f50d1b2126', 2644292, 'Beans, pinto, canned, sodium added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 6.69, 83.1, 210.0, 202.0, 55.2, 27.5, 1.28, 0.625, FALSE, NULL),
('f0264207-1519-4d07-b0e4-65a1be20b2a7', 2644293, 'Blackeye pea, canned, sodium added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 6.92, 83.0, 138.0, 227.0, 27.8, 27.3, 1.08, 0.655, FALSE, NULL),
('4b2c3785-87b9-412a-92d3-2fffac9d01af', 2644294, 'Beans, great northern, canned, sodium added, drained and rinsed', 'Legumes and Legume Products', 100.0, 0.0, 7.03, 88.6, 213.0, 223.0, 67.2, 29.3, 1.38, 0.635, FALSE, NULL),
('86b00e0c-8679-4274-996a-af7da5f1671b', 2646168, 'Pork, loin, boneless, raw', 'Pork Products', 100.0, 0.0, 21.1, 197.0, 361.0, 40.2, 4.11, 22.0, 0.452, 1.57, FALSE, NULL),
('db7ab9c4-142b-4c33-b84f-9fa60e8b24a5', 2646169, 'Pork, loin, tenderloin, boneless, raw', 'Pork Products', 100.0, 0.0, 21.6, 217.0, 397.0, 41.4, 4.53, 24.7, 0.929, 1.77, FALSE, NULL),
('8adbf052-5285-423b-a9b0-9e7fc1e0367b', 2646170, 'Chicken, breast, boneless, skinless, raw', 'Poultry Products', 100.0, 0.0, 22.5, 215.0, 330.0, 65.8, 3.94, 26.2, 0.354, 0.654, FALSE, NULL),
('449ce785-f355-4d52-9fd2-668fb09541ee', 2646171, 'Chicken, thigh, boneless, skinless, raw', 'Poultry Products', 100.0, 0.0, 18.6, 178.0, 272.0, 62.3, 5.65, 21.8, 0.602, 1.35, FALSE, NULL),
('6960e381-c8d4-4b09-b6a2-c61ec4be5cf6', 2646172, 'Beef, ribeye, steak, boneless, choice, raw', 'Beef Products', 100.0, 0.0, 18.7, 150.0, 288.0, 42.5, 4.17, 16.7, 1.64, 4.06, FALSE, NULL),
('49985801-1e3b-47cb-a915-7f9553b409e7', 2646173, 'Beef, round, top round, boneless, choice, raw', 'Beef Products', 100.0, 0.0, 21.5, 192.0, 352.0, 45.5, 4.07, 22.1, 1.9, 3.78, FALSE, NULL),
('4c356049-338a-4ae3-8851-ed442c57e694', 2646174, 'Beef, chuck, roast, boneless, choice, raw', 'Beef Products', 100.0, 0.0, 18.4, 151.0, 281.0, 48.4, 4.58, 17.0, 2.06, 5.39, FALSE, NULL),
('c4c10aac-f7cf-4abd-9651-2917401bd0e2', 2646175, 'Beef, flank, steak, boneless, choice, raw', 'Beef Products', 100.0, 0.0, 20.1, 167.0, 332.0, 51.3, 3.84, 18.9, 1.83, 5.56, FALSE, NULL),
('e230afa0-ee90-4b37-87c2-778a34f4d5bd', 2647437, 'Yogurt, plain, nonfat', 'Dairy and Egg Products', 100.0, 0.0, 4.23, 127.0, 210.0, 51.5, 167.0, 15.2, 0.0, 0.599, FALSE, NULL),
('56e351a1-f3bd-4c6c-aba8-0d3b9a4cc74e', 2647438, 'Cheese, monterey jack, solid', 'Dairy and Egg Products', 100.0, 0.0, 22.6, 471.0, 82.9, 662.0, 715.0, 29.6, 0.0, 3.78, FALSE, NULL),
('78a454fe-9cb1-4f7a-b797-bc86b939e927', 2647439, 'Cheese, pasteurized process cheese food or product, American, singles', 'Dairy and Egg Products', 100.0, 0.0, 15.6, 880.0, 212.0, 1420.0, 1500.0, 34.6, 1.3, 2.38, FALSE, NULL),
('1c5dae43-8d0b-4e6b-8ab6-d25aa974c3bc', 2647440, 'Cheese, provolone, sliced', 'Dairy and Egg Products', 100.0, 0.0, 23.5, 494.0, 94.8, 601.0, 749.0, 29.5, 0.0, 3.89, FALSE, NULL),
('52ce2c7b-8f13-4dde-963c-25653ee99d82', 2647441, 'Cheese, oaxaca, solid', 'Dairy and Egg Products', 100.0, 0.0, 22.1, 380.0, 80.7, 734.0, 532.0, 21.6, 0.0, 3.51, FALSE, NULL),
('282d91f1-cdf2-48d3-97ba-c8f2b043049d', 2647442, 'Cheese, queso fresco, solid', 'Dairy and Egg Products', 100.0, 0.0, 18.9, 390.0, 126.0, 626.0, 602.0, 27.6, 0.0, 2.91, FALSE, NULL),
('43844bd6-1718-42ed-a8c3-9ece1a69cd66', 2647443, 'Cheese, cotija, solid', 'Dairy and Egg Products', 100.0, 0.0, 23.8, 486.0, 117.0, 1620.0, 700.0, 29.9, 0.0, 3.6, FALSE, NULL),
('7c1bd87b-82a1-4f76-9a6d-e7ec6f47cb76', 2684440, 'Fish, salmon, sockeye, wild caught, raw', 'Finfish and Shellfish Products', 100.0, 0.0, 22.3, 226.0, 330.0, 53.3, 14.8, 26.2, 0.402, 0.425, FALSE, NULL),
('b0eeacf5-3efe-426a-8504-90ede62e5752', 2684441, 'Fish, salmon, Atlantic, farm raised, raw', 'Finfish and Shellfish Products', 100.0, 0.0, 20.3, 230.0, 378.0, 49.5, 9.42, 25.4, 0.259, 0.339, FALSE, NULL),
('4d362779-2a4a-4275-8b4c-f4460cc39ac7', 2684442, 'Fish, tilapia, farm raised, raw', 'Finfish and Shellfish Products', 100.0, 0.0, 19.0, 159.0, 342.0, 93.7, 8.63, 24.3, 0.0, 0.3, FALSE, NULL),
('633b30dc-f431-4d19-bff7-04ebfa76126d', 2684443, 'Crustaceans, shrimp, farm raised, raw', 'Finfish and Shellfish Products', 100.0, 0.0, 15.6, 191.0, 146.0, 475.0, 64.6, 22.5, 0.522, 0.937, FALSE, NULL),
('57b0e5cc-2d38-4039-b2c2-c06836613db3', 2684444, 'Fish, cod, Atlantic, wild caught, raw', 'Finfish and Shellfish Products', 100.0, 0.0, 16.1, 224.0, 245.0, 299.0, 6.59, 18.0, 0.065, 0.305, FALSE, NULL),
('0ba8b17f-6604-448a-a299-41e784802c90', 2684445, 'Fish, catfish, farm raised, raw', 'Finfish and Shellfish Products', 100.0, 0.0, 16.5, 166.0, 292.0, 60.5, 7.8, 20.6, 0.0, 0.475, FALSE, NULL),
('34a1ddf1-8057-4ef9-86f6-9896b1a39757', 2684446, 'Crustaceans, crab, blue swimming, lump, pasteurized, refrigerated', 'Finfish and Shellfish Products', 100.0, 0.0, 18.6, 278.0, 235.0, 331.0, 111.0, 43.1, 0.632, 3.48, FALSE, NULL),
('0f66a854-3bdf-49ff-9420-04a67d5d17d4', 2685568, 'Squash, summer, green, zucchini, includes skin, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.984, 32.9, 226.0, 0.0, 20.8, 15.3, 0.194, 0.26, FALSE, NULL),
('6e56c867-d9f8-4502-b3b4-240d813fdc6c', 2685569, 'Squash, summer, yellow, includes skin, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.891, 30.7, 220.0, 0.0, 22.7, 16.8, 0.137, 0.203, FALSE, NULL),
('d5e84929-27bf-418b-9d60-9b58c09f2334', 2685570, 'Squash, winter, butternut, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.15, 26.5, 329.0, 0.0, 21.7, 15.0, 0.213, 0.193, FALSE, NULL),
('56bf7422-ac21-45a5-9b78-b3b7fde040ea', 2685571, 'Squash, winter, acorn, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.25, 42.0, 332.0, 0.0, 24.8, 23.8, 0.308, 0.294, FALSE, NULL),
('714369ae-800f-4802-9fbc-ce4d9bb6d6cf', 2685572, 'Cabbage, bok choy, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.02, 30.5, 228.0, 14.3, 61.9, 13.6, 0.439, 0.274, FALSE, NULL),
('50de6007-8a04-4737-8426-2947c9c35ef7', 2685573, 'Cauliflower, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.64, 39.7, 274.0, 20.0, 20.4, 14.2, 0.334, 0.234, FALSE, NULL),
('94b5707c-5e39-4ba4-b8d1-7529d350882c', 2685574, 'Collards, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.97, 45.0, 410.0, 18.4, 276.0, 49.5, 0.75, 0.459, FALSE, NULL),
('14242608-3f58-4a5f-9642-1ecc43e348ec', 2685575, 'Brussels sprouts, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 3.98, 78.5, 477.0, 25.7, 38.6, 25.0, 0.73, 0.366, FALSE, NULL),
('d1421d9c-c87e-45e0-8145-606a6092ef89', 2685576, 'Beets, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.69, 33.6, 342.0, 112.0, 13.8, 21.0, 0.428, 0.31, FALSE, NULL),
('a1bba323-8527-41f0-8617-0327f49509f9', 2685577, 'Eggplant, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.852, 22.8, 222.0, 0.442, 11.1, 13.5, 0.0, 0.119, FALSE, NULL),
('cbad6251-2bbe-4987-8927-e2484360dffe', 2685578, 'Tomatoes, whole, canned, solids and liquids, with salt added', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.868, 17.3, 203.0, 112.0, 19.8, 10.8, 0.929, 0.12, FALSE, NULL),
('926217f2-cb16-4af5-8668-fe3f0156377f', 2685579, 'Tomato, sauce, canned, with salt added', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.35, 29.2, 356.0, 417.0, 17.1, 18.7, 1.11, 0.183, FALSE, NULL),
('ff16ef9f-c8eb-47af-b1bb-acaf0c06edc5', 2685580, 'Tomato, paste, canned, without salt added', 'Vegetables and Vegetable Products', 100.0, 0.0, 4.23, 91.2, 972.0, 61.2, 36.8, 49.6, 3.18, 0.511, FALSE, NULL),
('8a2ba283-954b-43a5-8711-100b14a7da01', 2685581, 'Tomatoes, crushed, canned', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.44, 32.9, 346.0, 140.0, 19.2, 18.6, 2.27, 0.201, FALSE, NULL),
('d0a7ba8d-0f97-47b3-a859-ab3632321c39', 2685582, 'Tomato, puree, canned', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.58, 34.1, 416.0, 29.4, 17.6, 19.3, 1.24, 0.208, FALSE, NULL),
('f1f51201-d189-43eb-9c26-aa700b28cca9', 2710815, 'Apricot, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.961, 22.5, 231.0, 0.0, 11.6, 8.88, 0.163, 0.148, FALSE, NULL),
('2429581d-28c5-4fde-9830-8d96978bebaa', 2710816, 'Melons, honeydew, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.531, 11.7, 209.0, 21.2, 6.58, 9.5, 0.0, 0.06, FALSE, NULL),
('25866128-6503-42fb-af1b-4804d9d87612', 2710817, 'Plantains, ripe, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.16, 30.1, 396.0, 0.0, 3.82, 33.9, 0.319, 0.147, FALSE, NULL),
('55a3b1fa-0b5b-4a23-b16d-a46e296a2225', 2710818, 'Plantains, underripe, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.23, 27.9, 406.0, 0.0, 4.83, 29.5, 0.334, 0.185, FALSE, NULL),
('4f45504e-3d3c-4ab5-a27f-a48bd62cda95', 2710819, 'Chia seeds, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 17.0, 691.0, 642.0, 0.0, 595.0, 326.0, 6.04, 5.56, FALSE, NULL),
('422f17ac-6b43-4293-844d-32de3f9901d6', 2710820, 'Bulgur, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 11.8, 312.0, 358.0, 2.18, 34.1, 119.0, 2.62, 2.15, FALSE, NULL),
('b562eb81-047d-4341-a14f-c0d047cefc1e', 2710821, 'Wild rice, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 12.8, 355.0, 299.0, 1.01, 7.97, 108.0, 1.53, 5.84, FALSE, NULL),
('4bfbca66-59b7-4541-afe4-7ca98ab54e6f', 2710822, 'Arugula, baby, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.65, 51.2, 407.0, 86.9, 204.0, 42.6, 1.39, 0.348, FALSE, NULL),
('e5ab4dd5-902c-4c34-986f-332611b70035', 2710823, 'Asparagus, green, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.44, 53.9, 278.0, 2.46, 20.6, 13.8, 0.444, 0.596, FALSE, NULL),
('86dd1f19-2e60-49d3-a184-adc24e02fd0c', 2710824, 'Avocado, Hass, peeled, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.81, 42.4, 576.0, 0.0, 14.5, 32.8, 0.613, 0.46, FALSE, NULL),
('2e28fa1d-4a6e-4ba8-a55e-3dec6968f3be', 2710825, 'Rice, black, unenriched, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 7.57, 307.0, 256.0, 0.0, 14.4, 113.0, 1.12, 1.72, FALSE, NULL),
('5ccbc08b-67cf-4d0b-beb8-3cb238c4269d', 2710826, 'Corn, sweet, yellow and white kernels,  fresh, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 2.79, 75.5, 237.0, 0.0, 0.656, 25.8, 0.388, 0.556, FALSE, NULL),
('247cb0d4-bc2d-4b28-9690-c18b940c83d2', 2710827, 'Einkorn, grain, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 15.1, 436.0, 432.0, 0.0, 41.4, 140.0, 3.69, 5.23, FALSE, NULL),
('15f9d632-24ec-41e3-a5c7-2518d6a84766', 2710828, 'Farro, pearled, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 12.6, 368.0, 385.0, 0.845, 25.9, 119.0, 3.2, 3.97, FALSE, NULL),
('5c52f24d-d242-4382-ab1e-dc5b18b07eb8', 2710829, 'Fonio, grain, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 7.17, 93.2, 43.8, 2.97, 11.6, 39.1, 2.66, 1.85, FALSE, NULL),
('361b67a2-5e54-4010-8af8-12b3769c6466', 2710830, 'Khorasan, grain, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 14.8, 371.0, 450.0, 3.94, 24.1, 130.0, 3.87, 4.46, FALSE, NULL),
('78c028ab-e626-47ac-a64d-28e6fe4bcecf', 2710831, 'Kiwifruit (kiwi), green, peeled, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.01, 27.6, 302.0, 2.28, 24.4, 14.8, 0.037, 0.455, FALSE, NULL),
('b6a54cb6-2b22-4048-8c18-bbe1d3ae197c', 2710832, 'Mandarin, seedless, peeled, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.04, 19.7, 167.0, 0.0, 44.0, 12.6, 0.0, 0.069, FALSE, NULL),
('4fdd2de5-2a1b-4469-8b73-f04e6dd5f1aa', 2710833, 'Mango, Tommy Atkins, peeled, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.562, 16.4, 165.0, 0.0, 12.4, 10.3, 0.0, 0.066, FALSE, NULL),
('259ce4fd-7bd6-4b9c-b781-cc215d7be6ed', 2710834, 'Mango, Ataulfo, peeled, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.688, 17.7, 204.0, 0.0, 10.0, 10.6, 0.0, 0.102, FALSE, NULL),
('043f1148-2287-4377-8026-7390f5f8f095', 2710835, 'Corn flour, masa harina, white or yellow, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 7.56, 241.0, 277.0, 2.73, 112.0, 93.3, 1.7, 1.62, FALSE, NULL),
('050f8308-1e5c-4839-b19e-86968e65b4b2', 2710836, 'Pear, Anjou, green, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.312, 12.3, 122.0, 0.398, 9.99, 6.49, 0.0, 0.107, FALSE, NULL),
('28c16b72-a83f-4c49-a647-7903ebf28017', 2710837, 'Plum, black, with skin, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.578, 18.6, 186.0, 0.0, 4.03, 6.62, 0.0, 0.045, FALSE, NULL),
('eaea2c3d-5e58-425c-a7c2-185083a4d343', 2710838, 'Rice, red, unenriched, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 8.56, 314.0, 245.0, 0.438, 9.22, 126.0, 1.2, 2.56, FALSE, NULL),
('a6edd591-f6bb-4706-9811-e2ccc0f5f490', 2710839, 'Sorghum bran, white, unenriched, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 11.2, 517.0, 852.0, 2.92, 61.7, 274.0, 14.2, 4.4, FALSE, NULL),
('dc003ed4-6578-4bc3-a5f2-91108286c2f3', 2710840, 'Sorghum flour, white, pearled, unenriched, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 10.2, 246.0, 274.0, 0.0, 7.89, 107.0, 2.22, 1.32, FALSE, NULL),
('2421ae45-5d9f-4a91-b83a-1d166e1d32fb', 2710841, 'Sorghum grain, white, pearled, unenriched, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 10.2, 243.0, 274.0, 0.0, 7.45, 109.0, 2.04, 1.28, FALSE, NULL),
('0c2e333d-9830-418b-83bd-bbc30360edc3', 2710842, 'Sorghum, whole grain, white, dry, raw', 'Cereal Grains and Pasta', 100.0, 0.0, 10.1, 294.0, 367.0, 0.329, 14.9, 136.0, 3.9, 1.77, FALSE, NULL),
('ac489ae9-9d55-4137-b1a2-5ea94b2cbc61', 2710843, 'Plantains, overripe, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.17, 28.9, 408.0, 0.0, 3.9, 31.7, 0.315, 0.149, FALSE, NULL),
('1d2f6050-4789-406f-9b9c-66eb86e08e2a', 2727566, 'Chicken, drumstick, meat and skin, raw', 'Poultry Products', 100.0, 0.0, 18.4, 152.0, 244.0, 91.0, 8.28, 18.6, 0.666, 1.7, FALSE, NULL),
('843820a9-87e2-4dfa-a0a3-a1c62d4bceba', 2727567, 'Chicken, thigh, meat and skin, raw', 'Poultry Products', 100.0, 0.0, 17.1, 154.0, 246.0, 63.6, 5.73, 18.5, 0.586, 1.2, FALSE, NULL),
('82abcf24-3191-4919-847f-d1b9f1393df7', 2727568, 'Chicken, wing, meat and skin, raw', 'Poultry Products', 100.0, 0.0, 18.4, 133.0, 194.0, 84.1, 13.6, 16.5, 0.511, 1.18, FALSE, NULL),
('1b145490-c70f-4465-895f-3234f7ef7307', 2727569, 'Chicken, breast, meat and skin, raw', 'Poultry Products', 100.0, 0.0, 21.4, 204.0, 332.0, 48.1, 6.9, 25.4, 0.354, 0.624, FALSE, NULL),
('7d6fd4a4-1360-4e10-928d-9ed5b267c4d2', 2727570, 'Lamb, ground, raw', 'Lamb, Veal, and Game Products', 100.0, 0.0, 17.5, 156.0, 272.0, 53.4, 6.58, 17.5, 1.64, 3.13, FALSE, NULL),
('f15af3b0-50a6-4f69-a007-6c50a3374b23', 2727571, 'Bison, ground, raw', 'Lamb, Veal, and Game Products', 100.0, 0.0, 19.9, 166.0, 301.0, 55.7, 6.83, 18.5, 2.17, 3.76, FALSE, NULL),
('50786aea-565c-4c27-ad9a-88138b99ac8d', 2727572, 'Beef, short loin (NY strip steak), raw', 'Beef Products', 100.0, 0.0, 21.3, 171.0, 323.0, 43.1, 5.04, 19.5, 1.61, 3.28, FALSE, NULL),
('e796f002-6678-433a-b678-0325b97a5082', 2727573, 'Beef, tenderloin steak, raw', 'Beef Products', 100.0, 0.0, 21.1, 190.0, 345.0, 44.8, 4.09, 22.0, 2.53, 3.22, FALSE, NULL),
('1dfca014-3c68-458e-b1d5-1c1a55041f86', 2727574, 'Beef, top sirloin steak, raw', 'Beef Products', 100.0, 0.0, 22.0, 193.0, 349.0, 42.8, 3.7, 22.1, 2.22, 3.45, FALSE, NULL),
('e8df79d3-119e-458d-9091-57bfdb247739', 2727575, 'Pork, chop, center cut, raw', 'Pork Products', 100.0, 0.0, 22.8, 202.0, 366.0, 39.3, 4.12, 23.4, 0.421, 1.32, FALSE, NULL),
('eed1b75d-e9a6-48ae-a23f-2ba115c50eb3', 2727576, 'Pork, belly, with skin, raw', 'Pork Products', 100.0, 0.0, 15.2, 114.0, 208.0, 49.7, 4.2, 12.2, 0.382, 1.07, FALSE, NULL),
('7353b020-0653-4981-9b1c-7a65c4214c6a', 2727577, 'Pawpaw, peeled, seeded, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.15, 23.4, 221.0, 0.351, 9.91, 10.6, 0.075, 0.085, FALSE, NULL),
('02467187-75e8-40f5-8194-47d123df0d35', 2727578, 'Squash, pie pumpkin, peeled, seeded, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.854, 22.3, 472.0, 0.312, 16.4, 12.8, 0.063, 0.139, FALSE, NULL),
('1cb681c6-40f6-4ead-a9b0-c5e5f31c5bf6', 2727579, 'Squash, spaghetti, peeled, seeded, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.792, 17.0, 267.0, 0.312, 16.6, 13.3, 0.031, 0.105, FALSE, NULL),
('71fff9c0-f291-4b9c-9cc6-249e00d95716', 2727580, 'Rutabaga, peeled, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.888, 38.9, 267.0, 4.59, 42.0, 15.4, 0.137, 0.211, FALSE, NULL),
('c939d8af-fe31-45a4-9c14-903a4cfd6388', 2727581, 'Blackberries, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 1.53, 20.2, 167.0, 1.55, 15.4, 20.6, 0.208, 0.189, FALSE, NULL),
('7ccb5304-ea69-4fbb-9073-b7123bb9893c', 2727582, 'Tomatillos, dehusked, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.06, 24.2, 239.0, 0.311, 7.14, 13.5, 0.245, 0.159, FALSE, NULL),
('b9a20cdf-7e0d-4f22-9a5d-9a7242d403b9', 2727583, 'Cabbage, napa, leaf, destemmed, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.06, 34.4, 235.0, 12.6, 35.0, 11.3, 0.255, 0.253, FALSE, NULL),
('7a052a1c-6215-4343-aa45-33bdbfeae149', 2727584, 'Leeks, bulb and greens, root removed, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.47, 34.6, 319.0, 17.7, 51.4, 15.7, 0.761, 0.293, FALSE, NULL),
('a4c3afce-ba42-421d-aea3-129f9aa0c5c3', 2727585, 'Green onion, (scallion), bulb and greens, root removed, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.669, 27.9, 232.0, 9.95, 59.4, 18.9, 1.04, 0.248, FALSE, NULL),
('bf7bf319-ee38-4380-98fc-35fc16793480', 2727586, 'Shallots, bulb, peeled, root removed, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.38, 54.6, 252.0, 3.64, 26.1, 15.1, 0.332, 0.282, FALSE, NULL),
('f002b885-52d1-46cd-a95c-07900a87e991', 2727587, 'Juice, prune, shelf-stable', 'Fruits and Fruit Juices', 100.0, 0.0, 0.423, 19.0, 216.0, 10.3, 16.2, 15.2, 0.444, 0.122, FALSE, NULL),
('e7690435-c442-4696-97dd-b2a9dfa94b27', 2727588, 'Juice, pomegranate, from concentrate, shelf-stable', 'Fruits and Fruit Juices', 100.0, 0.0, 0.0, 8.94, 166.0, 4.04, 11.0, 6.86, 0.031, 0.082, FALSE, NULL),
('2b29d469-4a65-4d20-902f-6c8f2e6d86bd', 2727589, 'Juice, tart cherry, from concentrate, shelf-stable', 'Fruits and Fruit Juices', 100.0, 0.0, 0.147, 16.1, 170.0, 1.36, 16.2, 12.1, 0.203, 0.033, FALSE, NULL),
('a7d6a8c8-1742-4e4d-9672-18b9b402e58a', 2747652, 'Anchovies, canned in olive oil, with salt, drained', 'Finfish and Shellfish Products', 100.0, 0.0, 26.9, 226.0, 298.0, 5400.0, 240.0, 228.0, 2.69, 2.54, FALSE, NULL),
('8faa69c7-c60c-4af7-a338-50d1ba8866f7', 2747653, 'Beet greens, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.61, 26.8, 369.0, 280.0, 72.8, 74.4, 3.16, 0.315, FALSE, NULL),
('59295027-0787-415f-98eb-6f9f8d79a76c', 2747654, 'Cod, Pacific or Alaskan, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 14.2, 279.0, 192.0, 354.0, 9.01, 17.9, 0.0, 0.269, FALSE, NULL),
('26df0d0f-df73-493d-88ba-7b89d78f78b1', 2747655, 'Fennel, bulb, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.922, 33.4, 332.0, 49.0, 41.3, 15.0, 0.0, 0.141, FALSE, NULL),
('0709431f-0110-46b5-9206-f479348577d1', 2747656, 'Halibut, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 19.1, 217.0, 430.0, 108.0, 4.1, 23.1, 0.0, 0.327, FALSE, NULL),
('e42c1424-aa84-4948-909c-8bc208397721', 2747657, 'Lobster, tail only, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 13.0, 175.0, 213.0, 509.0, 71.6, 39.0, 0.173, 1.54, FALSE, NULL),
('a644adcf-e9e0-445d-8de1-b35793252796', 2747658, 'Mahi mahi, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 19.8, 224.0, 408.0, 51.9, 6.0, 28.0, 0.052, 0.296, FALSE, NULL),
('58f5a2ba-e1fc-4fa2-ad7c-f2cc9bb33ee0', 2747659, 'Parsnips, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.27, 62.5, 493.0, 0.491, 44.2, 22.0, 0.505, 0.328, FALSE, NULL),
('dd8f33f1-aa5b-4487-bc9d-be393522df68', 2747660, 'Peppers, banana or Hungarian wax, seeded, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.723, 21.8, 177.0, 0.0, 9.82, 10.2, 0.162, 0.13, FALSE, NULL),
('be33c63e-7088-411b-ad7a-6e33c3c43aa9', 2747661, 'Peppers, jalapeno, seeded, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.621, 19.1, 167.0, 0.0, 10.1, 8.95, 0.038, 0.094, FALSE, NULL),
('4af21edb-fd92-40fa-8002-1d8654e5bdb5', 2747662, 'Peppers, poblano, seeded, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.43, 22.4, 192.0, 0.0, 8.37, 10.7, 0.117, 0.128, FALSE, NULL),
('532cf6b9-2be5-4790-a4da-e047f0be1c34', 2747663, 'Peppers, serrano, seeded, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.856, 24.2, 224.0, 0.0, 12.9, 12.7, 0.057, 0.138, FALSE, NULL),
('1192e4ab-3b81-45a2-a3dc-99882365539e', 2747664, 'Radicchio, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 1.31, 35.5, 335.0, 8.08, 30.9, 13.0, 0.401, 0.2, FALSE, NULL),
('8d29588a-dea1-470a-8547-9b9af369ca0e', 2747665, 'Radishes, red, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.656, 17.9, 198.0, 51.2, 21.5, 8.94, 0.0, 0.104, FALSE, NULL),
('6911c1ef-4a37-417f-906e-41c3b28793e2', 2747666, 'Scallops, bay, Patagonian, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 16.4, 229.0, 292.0, 252.0, 14.0, 46.4, 0.172, 1.02, FALSE, NULL),
('2537285e-e8b5-4916-a254-8d86ad5cd2c0', 2747667, 'Scallops, sea, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 13.5, 214.0, 245.0, 313.0, 10.5, 30.6, 0.213, 1.0, FALSE, NULL),
('abc15834-3c24-4803-a38c-5c0d66098a13', 2747668, 'Sea bass, Chilean, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 14.9, 152.0, 236.0, 109.0, 6.82, 16.6, 0.0, 0.295, FALSE, NULL),
('4f37fb2a-99c8-4279-9af0-a22a53555477', 2747669, 'Snapper, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 20.7, 199.0, 349.0, 93.1, 13.9, 27.5, 0.201, 0.364, FALSE, NULL),
('6e2bc744-8d22-4483-a2c2-8c8e55a8e6b2', 2747670, 'Snow crab, legs only, frozen', 'Finfish and Shellfish Products', 100.0, 0.0, 15.5, 162.0, 193.0, 728.0, 97.8, 58.4, 0.276, 3.37, FALSE, NULL),
('080d62f0-f9ec-4a0d-abc5-73acf18e22c5', 2747671, 'Squid (calamari), frozen, tubes only', 'Finfish and Shellfish Products', 100.0, 0.0, 8.81, 63.8, 9.48, 272.0, 10.6, 10.5, 0.0, 0.527, FALSE, NULL),
('be80229d-6748-48d2-bdd3-c5ca9638c8dc', 2747672, 'Swordfish, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 19.2, 235.0, 414.0, 56.9, 3.62, 26.7, 0.096, 0.541, FALSE, NULL),
('67386990-5f89-4fc1-99ce-f1ed9277ac51', 2747673, 'Tuna, ahi or yellowfin, frozen, wild caught', 'Finfish and Shellfish Products', 100.0, 0.0, 24.7, 271.0, 420.0, 94.4, 3.19, 35.5, 0.591, 0.352, FALSE, NULL),
('1dc15cb8-bef5-48c4-a119-3e84585ff460', 2747674, 'Turnips, raw', 'Vegetables and Vegetable Products', 100.0, 0.0, 0.953, 31.3, 262.0, 12.8, 32.7, 10.4, 0.0, 0.156, FALSE, NULL),
('f8dd644b-3660-4fa9-bf8b-78c31858e44c', 2747675, 'Watermelon, seedless, flesh only, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.871, 17.1, 117.0, 0.0, 7.86, 11.4, 0.021, 0.102, FALSE, NULL),
('c7b67f33-e150-49bf-a6d8-9fe2adb2f87c', 2747676, 'Watermelon, seedless, rind only, raw', 'Fruits and Fruit Juices', 100.0, 0.0, 0.531, 20.7, 272.0, 3.76, 16.9, 9.67, 0.0, 0.121, FALSE, NULL)
ON CONFLICT (id) DO NOTHING;

-- Migrando 383 Porciones de alimentos...
INSERT INTO public.food_portions (id, food_id, measure_unit_name, amount, gram_weight, modifier) VALUES
('601fbcbe-97fd-4259-a0f2-96c895520c16', '1e9c9022-6f60-4cc8-ae96-068d8c1e72cb', 'tablespoon', 2.0, 33.9, ''),
('c5a2459f-219b-44e7-b9d7-ed8926fa79bd', '1e9c9022-6f60-4cc8-ae96-068d8c1e72cb', 'RACC', 1.0, 30.0, ''),
('ccba42af-5c0e-414e-a2d2-021d4a196738', 'fbc06787-3027-4be9-9b56-f27589223534', 'tomatoes', 5.0, 49.7, ''),
('b281ed0e-2f3d-4bcd-8829-950220861f1b', 'fbc06787-3027-4be9-9b56-f27589223534', 'cup', 1.0, 152.0, ''),
('fec514ce-a9db-4762-aa77-fbe32bcb6e9a', 'fbc06787-3027-4be9-9b56-f27589223534', 'RACC', 1.0, 85.0, ''),
('15370b45-e581-46da-a271-c5b63aab1c66', '2d3c605f-9d78-4d5a-87ba-967ac7f1fa1f', 'cup', 1.0, 129.0, 'drained'),
('8b8bc49b-a4f3-4839-9241-68e18a769fcf', '2d3c605f-9d78-4d5a-87ba-967ac7f1fa1f', 'RACC', 1.0, 130.0, ''),
('880efaeb-d88c-41c9-8521-d392bc04a65f', '86bc73c0-8966-4d0a-91f2-6dec9aaeeae1', 'piece', 1.0, 48.6, ''),
('aa2108fa-dfcd-4be0-9713-d78b391d0bae', '86bc73c0-8966-4d0a-91f2-6dec9aaeeae1', 'RACC', 1.0, 85.0, ''),
('5689e314-c935-4a65-b95b-0d5b7c2dafdd', 'd3a4818b-eac0-4025-9022-c8287a232b8a', 'cup', 1.0, 135.0, 'whole'),
('74b4c40e-7962-4ced-bd99-84ddb7d5fcad', 'd3a4818b-eac0-4025-9022-c8287a232b8a', 'RACC', 1.0, 30.0, ''),
('1531235b-e670-4123-a868-a268ac6733a8', '1f01cfbc-38fa-428f-bb0d-e121ea2c6bbb', 'cup', 1.0, 20.6, 'pieces of ~1"'),
('c05a9b37-f501-4e38-bd0e-96d326506ba5', '1f01cfbc-38fa-428f-bb0d-e121ea2c6bbb', 'RACC', 1.0, 85.0, ''),
('8a4db7f0-82ba-4e80-bc89-37beb60bcdb2', 'c1c3e7dd-1b3e-405d-9c67-985e89f0ce28', 'oz', 1.0, 28.4, ''),
('ec0dea6c-9785-4e4e-b3d1-a4be7f9cf5b0', 'c1c3e7dd-1b3e-405d-9c67-985e89f0ce28', 'RACC', 1.0, 50.0, ''),
('1f18600e-01ab-41c7-9d63-10421d427f9c', '7a931c4e-ef20-4790-b395-a439af7aee7a', 'oz', 1.0, 28.4, ''),
('67a47ca3-465e-4928-8654-7c0d513b3463', 'a8f914a4-a87f-4c83-bd44-b49ef0f17600', 'tablespoon', 1.0, 7.0, ''),
('84fe166d-0bd1-4bac-a183-ce7be8edc2d4', 'a8f914a4-a87f-4c83-bd44-b49ef0f17600', 'cup', 1.0, 107.0, 'sifted'),
('cd121965-cd05-44f4-a2e0-77add8d5f0c4', '1a79d64d-dc7c-45ef-b43b-a6a58bd412f8', 'piece', 1.0, 20.2, ''),
('e004b2c9-4bb4-446f-85ef-3841be13630a', '1a79d64d-dc7c-45ef-b43b-a6a58bd412f8', 'RACC', 1.0, 85.0, ''),
('c0666c72-f12a-419c-b3a1-e0124655cd2a', '6d484f7a-247e-40c5-ab00-833412220441', 'spear', 1.0, 40.4, ''),
('4b4e8ef9-7b34-4cd6-af41-3533dd0f0beb', '6d484f7a-247e-40c5-ab00-833412220441', 'RACC', 1.0, 30.0, ''),
('2567830f-6c54-4fa9-b310-632b9049b9b2', 'a83e811b-4aef-4e16-bb4d-ffa0b27d0961', 'tablespoon', 1.0, 7.6, ''),
('9939aee2-59f7-46ff-8171-9b58340ba102', 'a83e811b-4aef-4e16-bb4d-ffa0b27d0961', 'RACC', 1.0, 5.0, ''),
('0dc6142d-24dc-4404-8920-486832c38830', '09e51e3a-3733-4c3c-9a55-3fadbc674b8c', 'cup', 1.0, 257.0, ''),
('af3d9e71-d577-4a11-84eb-d8bfb28e016b', '1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 'fruit', 1.0, 147.0, 'small (2-1/2" dia)'),
('5323b5ea-1bec-47e5-aad2-a044b517fab3', '1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 'cup', 1.0, 154.0, 'slices with skin'),
('c4eff646-39e7-4bd2-9378-1407fcf6fdd6', '1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 'fruit', 1.0, 173.0, 'large (2-3/4" dia)'),
('edeaac85-d919-4c07-b6b9-9fef73154a21', '1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 'fruit', 1.0, 225.0, 'extra large (3" dia)'),
('5eb0a837-ecba-4d95-972b-fbd8d59c70c4', '1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 'fruit', 1.0, 161.0, 'medium (2-2/3" dia)'),
('8db523e1-c590-4934-9a98-2f633f78d65a', '1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 'serving', 1.0, 147.0, 'NLEA '),
('a5b57fef-276c-4d33-b0cb-7149ed801a4b', '1094c82b-a80c-41b4-b9a4-03b0d34c1ceb', 'RACC', 1.0, 140.0, ''),
('577354e4-44b6-457c-a5f7-5c1c961ece79', '997b7f99-0431-4731-b3d1-f7afbf49805a', 'cup', 1.0, 127.0, ''),
('c353b640-9d29-4592-a6d3-9591d3ace6f5', '997b7f99-0431-4731-b3d1-f7afbf49805a', 'RACC', 1.0, 30.0, ''),
('4aaed67f-e6ac-486c-ad0d-6e5a547f92a9', '8b2448f4-197a-45ea-aa8d-a7741f20829f', 'cup', 1.0, 118.0, 'cooked'),
('43d1400e-8063-458c-bc34-66ff4802cf6b', '8b2448f4-197a-45ea-aa8d-a7741f20829f', 'RACC', 1.0, 85.0, ''),
('4924e15f-ea0e-4046-aa76-924611fab0d9', '78accd46-ad8f-4923-8513-9d53b2cbda06', 'cup', 1.0, 249.0, ''),
('8d28d83c-82d4-446d-8f6f-7382208978b6', '78accd46-ad8f-4923-8513-9d53b2cbda06', 'teaspoon', 1.0, 6.0, ''),
('a72dcb69-492a-4f04-83bb-4835f8ac3487', 'c26f31cf-3d80-4e1c-816d-09c26239855e', 'RACC', 1.0, 140.0, ''),
('cee4ee6b-d3f8-427c-a37a-80b321085140', 'bef57fc3-fb8f-48e3-bdff-9fa16c1d2a39', 'each', 1.0, 129.0, '2-1/3" dia'),
('a68b854c-e927-429d-a270-57ed8c1ba027', 'bef57fc3-fb8f-48e3-bdff-9fa16c1d2a39', 'each', 1.0, 156.0, '2-3/4" dia'),
('6fffc803-a9df-4001-9e40-14e6b862d2aa', 'bef57fc3-fb8f-48e3-bdff-9fa16c1d2a39', 'each', 1.0, 142.0, ' 2-1/2" dia'),
('a6c5ffa5-9f8d-4ebd-9f82-1862c76800dc', 'bef57fc3-fb8f-48e3-bdff-9fa16c1d2a39', 'serving', 1.0, 140.0, 'NLEA'),
('1ca16974-ab05-427a-b5a2-6c59e3917d52', 'bef57fc3-fb8f-48e3-bdff-9fa16c1d2a39', 'cup', 1.0, 143.0, ' slices'),
('93208959-20d5-478a-8d4d-033705c2e68c', 'bef57fc3-fb8f-48e3-bdff-9fa16c1d2a39', 'RACC', 1.0, 140.0, ''),
('bb45d7e5-f2c2-4573-bde3-8e8d9058f4e7', 'c9a60ea0-e148-43da-942f-121b2310d94c', 'slice', 1.0, 17.0, ''),
('89fd6776-73db-45f7-9530-c53570ea3225', 'c9a60ea0-e148-43da-942f-121b2310d94c', 'cup', 1.0, 105.0, ''),
('adac8c69-90fa-434e-bcd7-3470731db628', 'c9a60ea0-e148-43da-942f-121b2310d94c', 'RACC', 1.0, 30.0, ''),
('607e9afe-140c-4e8c-8299-661e0592693e', '3f48ab43-0ee3-4369-8aa8-b059145abb86', 'cup', 1.0, 220.0, ''),
('9b29a5df-7b80-4b7f-a617-756faffba3c2', '3f48ab43-0ee3-4369-8aa8-b059145abb86', 'RACC', 1.0, 55.0, ''),
('8a3a0584-c22d-4117-9a13-201bc24dcb2b', '428d2964-65b4-4a76-b70e-e4c59c27b8f4', 'cup', 1.0, 85.0, 'sifted'),
('fff1ef47-316e-4e28-99f2-c5feaa5f55ad', '428d2964-65b4-4a76-b70e-e4c59c27b8f4', 'tablespoon', 1.0, 5.0, ''),
('ef0b73d6-d7bc-4d57-b309-9ef62d394228', '428d2964-65b4-4a76-b70e-e4c59c27b8f4', 'oz', 1.0, 28.4, ''),
('410f1c6f-eda5-42d7-881d-b68353a0aad2', '42459d75-369a-41c1-b7fd-29fb210f89c3', 'oz', 1.0, 28.4, ''),
('e9e8fcb4-b296-440c-811f-88135a492950', 'ba8e0002-ac38-42f9-8ce9-7353e876cd2a', 'tablespoon', 1.0, 4.0, ''),
('aefc2d10-c75b-48a1-b563-954c0cf78932', 'ba8e0002-ac38-42f9-8ce9-7353e876cd2a', 'cup', 1.0, 67.0, 'sifted'),
('3fc7f3de-c54e-466f-b2b9-6fbe8f2742b1', 'ba8e0002-ac38-42f9-8ce9-7353e876cd2a', 'oz', 1.0, 28.4, ''),
('52a31f66-ca61-47d0-adba-141914c1d1f0', '04df975e-d4d3-45ba-8eb8-ea78106a00c1', 'container', 1.0, 156.0, ''),
('9f89a9ec-ca81-44ab-b803-a8b66f23a6d6', '04df975e-d4d3-45ba-8eb8-ea78106a00c1', 'RACC', 1.0, 170.0, ''),
('f7d2963c-cd64-4657-b0c8-6fe162538209', 'bb28429f-341a-4ab8-bb42-8946c227236b', 'container', 1.0, 150.0, '5.3 oz'),
('3337a59a-5795-4714-af95-4d24bf17c5cb', 'bb28429f-341a-4ab8-bb42-8946c227236b', 'RACC', 1.0, 170.0, ''),
('e1af6ba1-0a59-4661-9437-ecf85561221e', 'a6fb12bd-4ceb-4644-bd91-f50e11a603e2', 'tablespoon', 1.0, 11.6, 'liquid oil'),
('b3184a3f-de03-4554-bcdd-61886fc76f45', '9b24274e-eecb-424a-916b-8216bb73efef', 'drumstick', 1.0, 104.0, 'with skin'),
('238cd276-f711-411f-9b4f-a484d07c7338', '9b24274e-eecb-424a-916b-8216bb73efef', 'drumstick', 1.0, 94.7, 'without skin'),
('56c9d454-171b-4c9d-82c4-a79e8064ae2f', '9b24274e-eecb-424a-916b-8216bb73efef', 'RACC', 1.0, 114.0, ''),
('9e8e7398-0f17-4d11-ae58-7d2c52618beb', 'fb3575a0-8e89-4824-98e6-bed6412058dd', 'piece', 1.0, 174.0, ''),
('aa92eb09-08c1-4a1e-a5f4-b1c72e4b704a', 'fb3575a0-8e89-4824-98e6-bed6412058dd', 'RACC', 1.0, 114.0, ''),
('2d2ccea1-880c-4329-8554-1aac69b5c589', '35d0c379-3a08-4548-b9fe-b917639c86d7', 'slice', 1.0, 13.5, ''),
('d9b841ed-910b-471d-863a-0f69b1b9f1b7', '35d0c379-3a08-4548-b9fe-b917639c86d7', 'RACC', 1.0, 85.0, ''),
('9b08e9d9-ef0e-4593-a827-6a405edf1601', 'c80ef17d-a505-4b6f-8f6a-0cf11893631b', 'olive', 1.0, 3.2, ''),
('303b6cd8-81e3-450e-a3af-1740b7ab469b', 'c80ef17d-a505-4b6f-8f6a-0cf11893631b', 'RACC', 1.0, 40.0, ''),
('68f88493-0899-444f-88e3-c1adbc7a4002', '8683890f-79aa-4b0e-8d73-6b32882d4f0c', 'cookie', 1.0, 27.0, ''),
('a973357f-4ac2-4869-a8b3-5bf8bbc7cde4', '8683890f-79aa-4b0e-8d73-6b32882d4f0c', 'RACC', 1.0, 30.0, ''),
('5d02faab-c923-4a67-aa2c-b449f3089fac', 'f51f07f4-3b62-4247-a181-b5b510541078', 'cup', 1.0, 245.0, ''),
('340c22d9-01c1-4947-8798-3649aefdb0cd', 'f51f07f4-3b62-4247-a181-b5b510541078', 'RACC', 1.0, 130.0, ''),
('321d50ab-5900-43dc-b159-e4f51b945b80', '267e2083-92c9-4d25-84f2-736a3f7d44b8', 'fillet', 1.0, 176.0, ''),
('dc569a70-3361-4590-979c-6a55d4a0d787', '267e2083-92c9-4d25-84f2-736a3f7d44b8', 'RACC', 1.0, 115.0, ''),
('c946bf37-b597-4996-ab7e-49e6bfdc840e', 'd2d8402d-9d70-4bed-b38c-4615445e92b9', 'fillet', 1.0, 177.0, ''),
('e047ca8a-3b70-41e1-9eff-100742f716b4', 'd2d8402d-9d70-4bed-b38c-4615445e92b9', 'RACC', 1.0, 115.0, ''),
('bf275f28-e12a-4aa4-a704-0db569dec66a', '4de7d3fa-e504-4e7a-b68e-421d565bc761', 'can', 1.0, 142.0, 'total can contents'),
('c726274b-f865-4e5b-b848-334709a48451', '4de7d3fa-e504-4e7a-b68e-421d565bc761', 'can', 1.0, 107.0, 'drained solids'),
('add5bdb3-af83-4126-b93a-c0af7ec08568', '4de7d3fa-e504-4e7a-b68e-421d565bc761', 'RACC', 1.0, 85.0, ''),
('5fd40c07-1fb1-4d33-a332-6157ebdcb6ce', '53e73d9e-4654-406b-98ab-2dd13f638d0e', 'cup', 1.0, 133.0, ''),
('18f9538c-3031-47a2-9591-26d6995a2983', '53e73d9e-4654-406b-98ab-2dd13f638d0e', 'RACC', 1.0, 85.0, ''),
('e8496abc-d805-4b8b-82f5-6b46a5059c33', 'd176c137-a3ef-4c60-928d-4ff01ddb1ee3', 'piece', 1.0, 142.0, ''),
('c16e89d1-3627-4fbc-9fd9-c1c62f5fe66e', 'd176c137-a3ef-4c60-928d-4ff01ddb1ee3', 'RACC', 1.0, 140.0, ''),
('9792d996-a307-4b23-83ee-6de52d025e20', '8bc3aab6-ba11-43f3-8944-77538fe9b5eb', 'piece', 1.0, 126.0, ''),
('4262a9d0-7040-40ef-be2f-7789c2068218', '8bc3aab6-ba11-43f3-8944-77538fe9b5eb', 'RACC', 1.0, 140.0, ''),
('889ee670-7eda-4fd7-aa24-31d08b64184c', '069ebb77-b957-43a1-82ed-efd7141e039c', 'roast', 1.0, 462.0, 'cooked'),
('14c297b7-5b81-4e69-ab89-fb9cba51ee8f', '069ebb77-b957-43a1-82ed-efd7141e039c', 'RACC', 1.0, 114.0, ''),
('b8c41dfb-b79b-4a61-a200-9894d683f027', 'ec037f7f-18c1-443b-933a-c8a992b0af18', 'roast', 1.0, 690.0, 'raw'),
('9af09c01-d49a-41ca-bd97-c8681c572653', 'ec037f7f-18c1-443b-933a-c8a992b0af18', 'RACC', 1.0, 114.0, ''),
('32c61878-798a-47a3-bf96-528170022ad6', 'e73b94a9-3bed-44e3-88d7-67bbf40a2fd7', 'roast', 1.0, 788.0, 'raw'),
('ab04f0e6-a74d-4442-86df-c2dc16cfa537', 'e73b94a9-3bed-44e3-88d7-67bbf40a2fd7', 'RACC', 1.0, 114.0, ''),
('23e28a64-c1e0-41ee-879a-3c94168df257', '151c79c6-3b15-4b14-ae70-ab72dae1a2da', 'steak', 1.0, 525.0, 'raw'),
('4c6ea574-9e79-4584-8864-618b1cc9a35e', '151c79c6-3b15-4b14-ae70-ab72dae1a2da', 'RACC', 1.0, 114.0, ''),
('c74386d9-1785-47a4-b2d3-facf931575e8', '350c5bac-9459-4a3d-bf72-4ed33ab97ba6', 'steak', 1.0, 360.0, 'cooked'),
('25844373-c2f9-4d1a-8cab-0b8eccf0e93c', '350c5bac-9459-4a3d-bf72-4ed33ab97ba6', 'RACC', 1.0, 114.0, ''),
('3597b461-3f65-406c-94e4-a1dbf595d0e0', '8548ff7c-6d65-472e-9134-a852be999a2a', 'RACC', 1.0, 85.0, ''),
('8d9eaa7d-7eda-40c4-8024-8adc77e4789b', '03a34555-444e-4366-af68-277aa1442492', 'cup', 1.0, 97.3, 'grated'),
('b103743c-1e81-4bdd-b5dd-9d75927f8b94', '03a34555-444e-4366-af68-277aa1442492', 'RACC', 1.0, 30.0, ''),
('df47779e-5616-4b0b-935b-6ee74e4bf729', '7089472f-45a0-476f-be60-a0f8e62b1e6a', 'cup', 0.2, 64.6, ''),
('07d523b4-70d3-440e-8d82-92b38fd29d1c', '7089472f-45a0-476f-be60-a0f8e62b1e6a', 'cup', 1.0, 258.0, ''),
('40f0ca23-7b04-4def-8eaa-134cb5e24501', '7089472f-45a0-476f-be60-a0f8e62b1e6a', 'cup', 0.5, 129.0, ''),
('9bf2ae00-9dff-4c55-956b-a5fcf8642407', '7089472f-45a0-476f-be60-a0f8e62b1e6a', 'RACC', 1.0, 55.0, ''),
('a2b068e8-a693-40ef-b0fd-0b9f32818549', '8834aa8c-060d-43ca-9c8c-efdca2113e65', 'slice', 1.0, 21.9, 'regular'),
('85b5eddd-45e6-4412-9208-ce46951c4006', '8834aa8c-060d-43ca-9c8c-efdca2113e65', 'slice', 1.0, 10.9, 'thin'),
('2dd03b21-5092-48d5-8442-a8d011224ce7', '8834aa8c-060d-43ca-9c8c-efdca2113e65', 'RACC', 1.0, 30.0, ''),
('69f77458-06ad-431c-98b7-c544f562920b', '3962bd0f-10ed-4f34-bfbd-fdf2ba6bc440', 'each', 1.0, 8.4, ''),
('918ee22b-d34a-479f-92be-bae15a2a3dd7', '3962bd0f-10ed-4f34-bfbd-fdf2ba6bc440', 'cup', 1.0, 149.0, ''),
('f6d0fb19-5f7f-46ab-a950-32262c0de4b2', '3962bd0f-10ed-4f34-bfbd-fdf2ba6bc440', 'RACC', 1.0, 140.0, ''),
('aaf81221-bc79-41b2-9164-ab15f5133f4c', 'aecfdb91-fd73-4dc7-a81c-cb69fe339014', 'bunch', 1.0, 581.0, ''),
('1db3b4a4-cc67-4e48-a821-71b233158ca1', 'aecfdb91-fd73-4dc7-a81c-cb69fe339014', 'RACC', 1.0, 85.0, ''),
('f00c6804-5e0e-4164-af8b-8e6f97143a41', '04ec36a7-716e-4499-964c-d8474a7a6615', 'wedge', 1.0, 69.0, 'medium (1/8 of medium melon)'),
('6e961d3d-a429-4723-a7d1-70ad9b20c79e', '04ec36a7-716e-4499-964c-d8474a7a6615', 'wedge', 1.0, 102.0, 'large (1/8 of large melon)'),
('5c58ec79-f1aa-43d1-ac52-e9a060a5580b', '04ec36a7-716e-4499-964c-d8474a7a6615', 'each', 1.0, 441.0, 'small (about 4-1/4" dia)'),
('c17fa5d4-945d-44d0-aac8-c4ec26631b68', '04ec36a7-716e-4499-964c-d8474a7a6615', 'each', 1.0, 814.0, 'large (about 6-1/2" dia)'),
('646177de-a5b0-4a35-a598-42348495ded1', '04ec36a7-716e-4499-964c-d8474a7a6615', 'cup', 1.0, 160.0, 'cubes'),
('626b79ed-1d5a-4d6d-9709-1a0fcc174a9d', '04ec36a7-716e-4499-964c-d8474a7a6615', 'pieces', 10.0, 138.0, 'balls'),
('8317db79-63de-4c0a-8450-908625373980', '04ec36a7-716e-4499-964c-d8474a7a6615', 'cup', 1.0, 156.0, 'diced'),
('cd226695-2edd-4d2b-b931-a0d70602e73b', '04ec36a7-716e-4499-964c-d8474a7a6615', 'each', 1.0, 552.0, 'medium (about 5" dia)'),
('97ba6428-a000-45f9-9bb1-1bd9d35751d6', '04ec36a7-716e-4499-964c-d8474a7a6615', 'wedge', 1.0, 55.0, 'small (1/8 of small melon)'),
('e261f2e3-7e81-4a76-9127-3110673f3de0', '04ec36a7-716e-4499-964c-d8474a7a6615', 'RACC', 1.0, 140.0, ''),
('55f408e3-5fb4-41a4-afdd-61a7c1f4343a', 'c8ec5c7d-1866-4f39-877b-66b0a9c24654', 'cup', 1.0, 165.0, 'sections no membranes'),
('0b147a86-6b77-4c2d-90d4-ca6ffd251f14', 'c8ec5c7d-1866-4f39-877b-66b0a9c24654', 'each', 1.0, 140.0, ' 2-7/8" dia'),
('f34e11d3-f6be-496b-a035-d8b4238fa2a5', 'c8ec5c7d-1866-4f39-877b-66b0a9c24654', 'RACC', 1.0, 140.0, ''),
('722b3494-f054-454f-8ef0-719fb2133455', '2ca805b5-6d8d-48b4-a013-e3d5f1d0d436', 'cup', 1.0, 246.0, ''),
('21be7566-8051-4eef-a2ef-2ee156798033', '0fd7fed6-4539-45b5-8ea1-7b73a39573ae', 'cup', 1.0, 140.0, 'slices'),
('cb0493e9-93c8-4678-acc1-be5711e50de2', '0fd7fed6-4539-45b5-8ea1-7b73a39573ae', 'each', 1.0, 178.0, 'medium'),
('ff359d2a-d4af-42bc-bd43-d3e492b1ce7d', '0fd7fed6-4539-45b5-8ea1-7b73a39573ae', 'each', 1.0, 230.0, 'large'),
('23bcd419-a57a-4902-a958-a641efdde6d1', '0fd7fed6-4539-45b5-8ea1-7b73a39573ae', 'each', 1.0, 148.0, 'small'),
('9f22a092-7c25-4475-93b8-9e0e0a264dc4', '0fd7fed6-4539-45b5-8ea1-7b73a39573ae', 'RACC', 1.0, 140.0, ''),
('5ef259f3-c36e-4a3a-9eff-ba0f96dbac83', '5baf0db2-8c25-41e2-8e8f-d602f19ca8fb', 'order', 1.0, 611.0, ''),
('0e8ba5e2-8b23-4442-9ea4-faa8fb9f9f7f', '5baf0db2-8c25-41e2-8e8f-d602f19ca8fb', 'pieces', 3.0, 41.8, ''),
('679debb1-3296-48cc-b0c1-cc0ee9aa7df5', '5baf0db2-8c25-41e2-8e8f-d602f19ca8fb', 'RACC', 1.0, 195.0, ''),
('476e84b5-5c44-43af-b158-fe77657f5ed2', '59e266fa-1503-4123-83f7-b17f9e358113', 'teaspoon', 1.0, 6.1, ''),
('2eae919f-f367-462a-9411-2c197cedd914', '6a8a51f8-3fed-4f87-bd0c-105d7ec73c6e', 'cup', 1.0, 246.0, ''),
('9129b131-b49d-430f-bfa6-59dbdcab01a6', 'a2a0361a-7ac6-4698-935a-02b187df5f99', 'tablespoon', 2.0, 35.7, ''),
('282ebf8b-607f-48c1-9a04-3ac203205c73', '9d0765c9-27b4-4ba9-95f7-97b7390dbfb1', 'cup', 1.0, 245.0, ''),
('429ea903-9189-4f2e-a24a-fb8b75930033', 'f6eae660-139d-496c-8a57-cf9e1e3a52c3', 'link', 1.0, 18.6, ''),
('e0ad3b7b-3580-4cb8-81a0-e7ad063359ea', 'f6eae660-139d-496c-8a57-cf9e1e3a52c3', 'RACC', 1.0, 110.0, ''),
('bcffe3ca-3206-440f-ac06-c421d526e6b0', '1566d117-2ece-4d3d-9be5-986a459330df', 'link', 1.0, 86.6, 'medium'),
('cb446add-41cd-48e1-9be1-9999c77439e0', '1566d117-2ece-4d3d-9be5-986a459330df', 'RACC', 1.0, 85.0, ''),
('cc73a10c-eac5-4b1d-8d08-9217f3d90e0e', 'c4702eb1-d277-4fd5-9d19-4dc7ef071997', 'link', 1.0, 186.0, 'long'),
('0a91e337-739f-41f6-8c2e-1221262e353c', 'c4702eb1-d277-4fd5-9d19-4dc7ef071997', 'cup', 1.0, 185.0, 'ground'),
('c0ec92f2-b8cc-4f4e-9861-464a168cea5f', 'c4702eb1-d277-4fd5-9d19-4dc7ef071997', 'link', 1.0, 80.4, 'medium'),
('65bb5228-4a48-4a94-b5e8-146150e1a6fe', 'c4702eb1-d277-4fd5-9d19-4dc7ef071997', 'RACC', 1.0, 85.0, ''),
('2409c085-4fc6-41f5-ac53-d8ac00a38a54', 'f77aeb37-0df0-4429-a53f-d4a90c5cd386', 'cup', 1.0, 249.0, ''),
('cc47a4b8-ca9a-48a8-88dc-fd959d441c4c', 'b08e9a10-89de-46df-acec-e386bfc60e12', 'package', 1.0, 450.0, ''),
('d48bb8af-77c6-4df1-8bb1-ef540a983359', 'b08e9a10-89de-46df-acec-e386bfc60e12', 'link', 1.0, 27.9, ''),
('94a66ae7-4649-49fa-a98e-08a97f11b670', 'b08e9a10-89de-46df-acec-e386bfc60e12', 'RACC', 1.0, 110.0, ''),
('1c628858-1e5a-448e-bd21-96ed0797cf4c', '940647f4-2dbd-40b8-af17-d54a23fbc3a3', 'teaspoon', 1.0, 4.0, ''),
('e08b3af3-3f9e-4e3c-beda-562317e8fdef', '940647f4-2dbd-40b8-af17-d54a23fbc3a3', 'cup', 1.0, 188.0, ''),
('5284a186-a943-4aa4-af88-5b988c7f8073', '940647f4-2dbd-40b8-af17-d54a23fbc3a3', 'RACC', 1.0, 8.0, ''),
('cef03f72-fd42-431b-891d-a2f94014571d', '41134f4a-98d0-4100-a2bd-9dc49eabd33e', 'paired cooked w', 1.0, 155.0, 'crumbles'),
('8e70a64d-def3-4548-81b6-1fc980ab72e7', '41134f4a-98d0-4100-a2bd-9dc49eabd33e', 'RACC', 1.0, 114.0, ''),
('dea7be9d-ef39-4622-a32b-6c764a31146a', '1851cb0d-c7f7-4ba0-90b0-cf08ab363504', 'slice', 1.0, 16.2, ''),
('9363eec3-cc77-433a-b9f3-fdacaec9b6c6', '1851cb0d-c7f7-4ba0-90b0-cf08ab363504', 'RACC', 1.0, 85.0, ''),
('78ecbb3e-b577-4ae1-9c1e-360d21a3cc6b', 'f82aca4f-1f0d-4e97-b871-0707de835f77', 'cup', 1.0, 76.0, 'chopped'),
('6b3a2fb4-d14a-4657-9c21-2eb6f1f99d5e', 'f82aca4f-1f0d-4e97-b871-0707de835f77', 'RACC', 1.0, 85.0, ''),
('b74b1324-776a-477a-8fd6-5de835e28c7a', '52144cdc-83cd-4c06-88eb-9193d8d5dc73', 'egg', 1.0, 34.0, 'white'),
('b85916eb-1b8c-415b-ba07-b7513a1605e0', '52144cdc-83cd-4c06-88eb-9193d8d5dc73', 'RACC', 1.0, 50.0, ''),
('bdd1d16d-8747-4e6a-a46f-e67657dcd838', '72aa775d-54d3-4ec0-9d54-4aa472e55626', 'egg', 1.0, 17.0, 'yolk'),
('31fdfb03-5e68-400b-9d9b-0d6056bc2507', 'b7b90a8c-d6aa-4895-9ffd-4686be17a3fa', 'milliliter', 100.0, 90.9, ''),
('fa238574-8030-4a0f-8961-e01e5aa3dad9', 'dd0a5c34-9529-4697-9feb-d32834d1a872', 'milliliter', 100.0, 91.3, ''),
('1232c563-e57c-4d4c-9936-29184cb57a26', '3d246e86-3132-4557-b345-06ddde4f4fbe', 'milliliter', 100.0, 91.3, ''),
('3cd1a49d-4ed3-4798-9a94-969844e13b7c', 'a9070444-cf03-4e18-ac69-cc03d5d09d04', 'milliliter', 100.0, 90.7, ''),
('80fdd088-61c1-4c61-91c6-f8126e5493ae', '0249dbb6-1d4f-49c6-b78e-dd807ba28cf2', 'egg', 1.0, 50.3, 'whole without shell'),
('5f798b56-7aab-41d9-86f8-ed3bcdfc3ff8', '0249dbb6-1d4f-49c6-b78e-dd807ba28cf2', 'RACC', 1.0, 50.0, ''),
('8a8e8b86-edb5-429e-bb24-57b130e6d131', '2b79d3a8-25ee-423b-bc8b-047ed79c20f8', 'RACC', 1.0, 30.0, ''),
('e6729b31-772c-46cf-b32b-7f75dbe1c850', '8de6dc47-2ff9-4e1d-a445-5255a88127ce', 'RACC', 1.0, 30.0, ''),
('610bba6c-3838-4d50-bc9c-51c576538929', 'eb7ea36b-479d-4e69-a4ba-3f6f65011840', 'RACC', 1.0, 30.0, ''),
('37a0470d-2f66-4ff5-82bb-82ad42d4eb66', 'dfeb2854-f0ab-4bec-b3b0-f2c3775df999', 'RACC', 1.0, 30.0, ''),
('331c3e06-7601-4f08-bfed-dd54ba81abcf', 'b2203f15-0f87-45e5-95ae-6581389d3ed5', 'RACC', 1.0, 30.0, ''),
('5a316838-c8cc-4575-bfba-3a09cb4de8cb', '9913655d-485f-43c0-aa33-a58067a6c0f8', 'RACC', 1.0, 30.0, ''),
('518d729e-b35c-4d6e-b79c-8647839ce3de', '597b3e6e-1519-4387-a576-7b84aaca2638', 'RACC', 1.0, 30.0, ''),
('a2d694f1-4a17-486a-bc2c-259f20d00ec9', '7de0e82e-bcd4-4c0d-a4c4-9600c63a1426', 'Onion', 1.0, 197.0, 'Edible'),
('d3b8d280-9b59-4e64-a9d2-d6dafbcec315', '7de0e82e-bcd4-4c0d-a4c4-9600c63a1426', 'RACC', 1.0, 85.0, ''),
('939e0032-2604-4cda-a8df-a657a453ee65', 'c048ec2c-0f55-4d58-9c79-ca1f1b631ff3', 'Onion', 1.0, 143.0, 'Edible'),
('d839c82b-786a-4c5b-bff6-1e6572e1d389', 'c048ec2c-0f55-4d58-9c79-ca1f1b631ff3', 'RACC', 1.0, 85.0, ''),
('0466a5bb-1316-494c-870c-a4bb9c997f7a', 'c4e9a4fa-d6a8-4006-a1aa-e1c9c471baea', 'RACC', 1.0, 85.0, ''),
('f44b93e7-b05a-49e9-98af-afd9fa082fbd', 'a0d6ca11-a403-4474-8b1a-e1f494d340ff', 'RACC', 1.0, 30.0, ''),
('4553d0b2-bec9-47f4-bfa3-4b098ccea6f7', '477f02da-9552-4128-950f-3bf42a9c3dd2', 'RACC', 1.0, 30.0, ''),
('5d019d2e-ddad-4c73-b81f-4269885aec2f', '0f3da064-6540-40d2-8278-6c982d5fa5ff', 'RACC', 1.0, 30.0, ''),
('d5c59cc3-7a12-4b13-8940-fe23cc91067f', 'a7c2c24c-ce69-4042-a7b7-d51766508844', 'RACC', 1.0, 30.0, ''),
('f2936d09-7527-4407-bd50-d2862f10754d', 'bb92bd28-722f-4f6d-889a-bb63e012ca9d', 'RACC', 1.0, 30.0, ''),
('a3e31a1b-94d0-4734-97b5-c6f14e231821', '43f2668f-9145-4fad-b221-5d97236f03a2', 'RACC', 1.0, 85.0, ''),
('e01bd5a0-cd16-44cb-8c10-a0e76a3a55b6', 'e7232ac7-344a-495d-b71f-04c4b19f9d74', 'Banana', 1.0, 110.0, 'Peeled'),
('769b8f7e-a908-46ee-949e-89185b6464ca', 'e7232ac7-344a-495d-b71f-04c4b19f9d74', 'RACC', 1.0, 140.0, ''),
('282c9d06-c965-4a18-9eb7-25cf0447fae4', '6d67f8dd-8b21-4781-8a80-eba203866b1d', 'Banana', 1.0, 115.0, 'Peeled'),
('d10b7ad4-ba22-4551-b163-3cc7cc7d60a5', '6d67f8dd-8b21-4781-8a80-eba203866b1d', 'RACC', 1.0, 140.0, ''),
('ba6b9565-3cba-450d-91f6-1972de7040f5', '1eda1b7e-31f5-43d5-b56a-96c7fde3c136', 'RACC', 1.0, 140.0, ''),
('3fdabbc2-1817-4de0-99cf-2545a5be2a27', '2a8d4e8d-4c08-4a90-9de4-c60741c1b8f7', 'RACC', 1.0, 140.0, ''),
('3a7804dd-6444-4f6b-8f3a-347c3a176356', '4774bc26-f470-48ab-aa64-e9e6ea8bfdc2', 'RACC', 1.0, 140.0, ''),
('0798e318-e84d-4922-adb3-e29472f6605d', '221f0127-5212-4b47-9e88-ab8349470385', 'RACC', 1.0, 140.0, ''),
('c39e5b04-43df-4fe0-b086-0d1cdddce4fa', 'd276bf9d-a4a1-4887-870f-0cfe1556f717', 'RACC', 1.0, 140.0, ''),
('2b2b3311-4ff2-4720-8e31-e796431d94e7', '0d3c0dde-2917-4d7d-90a0-0dd0c7a09672', 'RACC', 1.0, 85.0, ''),
('4d417113-c70b-4d63-942e-58cb82815a0e', '9066f3b4-b0a3-40ad-83f6-2b2d9691ab1d', 'RACC', 1.0, 85.0, ''),
('c2a2a685-ae08-4ece-8ae2-8bc1ac9f3c31', '1b33988b-2f30-45cb-aa15-7e13d048db87', 'RACC', 1.0, 85.0, ''),
('b53a7750-848b-463e-b825-7c71aa3c23cc', '7192df84-a869-412c-853b-8345b46ca13f', 'RACC', 1.0, 85.0, ''),
('406df575-f1f1-4510-9839-b6d15f43d2c8', '991ac270-1540-4f11-ae52-550cdad1d387', 'RACC', 1.0, 85.0, ''),
('27dfcfeb-0951-48c8-b694-aafcc245d667', '39291176-6427-4f57-aae4-de0a9a6e0fc7', 'RACC', 1.0, 85.0, ''),
('2aeef573-11ea-4453-b0ff-cea993b6ae92', 'ac00fcdd-adcf-4871-876e-a08ab62f0812', 'RACC', 1.0, 85.0, ''),
('26d262dd-e663-4b20-a7ed-73a7380a2227', '5d4a5073-539c-46a2-8fc9-6153d12b7b6c', 'RACC', 1.0, 30.0, ''),
('ef78d4ba-8894-4098-a4cd-446b0f648d81', 'f2ba218f-7bf7-4a93-a7a5-a6dad2fa462a', 'RACC', 1.0, 30.0, ''),
('67ac3714-24be-48a0-a6b9-0b197782b974', 'a2d236f5-314a-427b-a819-0ee08a5daa63', 'RACC', 1.0, 30.0, ''),
('71bbd3e1-a643-4dc0-a352-32efd0536139', '1a23baf8-f825-4631-880f-dc34535cb957', 'RACC', 1.0, 30.0, ''),
('59ece801-37ef-4bb2-9307-914132a02282', 'fd88222a-f78e-4026-9c35-a0191541a764', 'RACC', 1.0, 85.0, ''),
('e649b9ca-0244-4935-a53b-cbd9d3ca8254', '68250641-44d6-4312-80b3-ca7f26b0b8da', 'RACC', 1.0, 85.0, ''),
('d9570f52-759b-4f9e-9960-c6fcb94447e6', '3fbd8d7c-6ebd-424f-b3c0-8dfc0c956af9', 'RACC', 1.0, 85.0, ''),
('8501afa4-5c9b-4878-baad-acf26871b0c2', '4e56c997-f6d0-4122-aeb0-c40f326b5efd', 'RACC', 1.0, 85.0, ''),
('bf27fa57-4842-4918-b080-6925787fb204', '808485f5-49b5-4c56-9619-3788de279d5b', 'RACC', 1.0, 85.0, ''),
('f23bb9b4-db16-46c7-b6c3-3bf9c4d75f87', '7ceaf18c-0f0c-445a-bcc0-0023365c0151', 'RACC', 1.0, 85.0, ''),
('36df1575-b296-437d-b352-7d9db5c38646', 'a0ca5d1e-c7de-4893-9ec4-5d0dc32a8aa8', 'RACC', 1.0, 85.0, ''),
('ed34f457-47de-4dd8-9107-5ff5681a5ba9', '44946c86-7f55-4ff7-93f9-f0c84fcb8105', 'RACC', 1.0, 85.0, ''),
('7ecaf4a2-724a-4786-b974-a3b85572e1c3', 'b4aaab90-235b-4393-8c5c-c7556b4d6c0e', 'RACC', 1.0, 85.0, ''),
('8565a27b-820b-45e7-8e25-cbaba68954e4', 'ce7cd6a6-8b41-4155-ac28-a0aee2b646ad', 'RACC', 1.0, 85.0, ''),
('b9108369-40c2-4ded-b208-17f2a4575baa', '45e83e57-5d4a-4b63-9cc1-94f736c72e93', 'RACC', 1.0, 85.0, ''),
('f91d6211-a602-4f70-a1e1-687a3a7cbd5d', 'c747f298-10b2-4bf8-b324-424338186df6', 'RACC', 1.0, 85.0, ''),
('71537f76-5c99-4297-9906-5c83cbe03b0b', '8243907b-79df-42ba-9c31-619f05bb25c6', 'RACC', 1.0, 85.0, ''),
('e4bcd9e3-1c85-4b2d-bbbf-40cd780bcecc', '8b04df5f-a6de-4026-b433-316c58ce049f', 'RACC', 1.0, 170.0, ''),
('76ecda3b-135b-4eaf-8c3f-93bdb692239e', '5e1cb88d-30b1-4c46-a39e-42d66a008d17', 'RACC', 1.0, 170.0, ''),
('e6b5fcbe-d148-41ec-ace8-453a4da8773b', 'afbf618e-cfac-4477-8f93-027285d82725', 'RACC', 1.0, 5.0, ''),
('d32a32dc-b276-4444-b136-50c3b1e239f8', '8851cb46-9876-45e8-95d7-a508e6ca4e55', 'RACC', 1.0, 30.0, ''),
('fa8c53c6-b0cc-4691-b019-5c9a4f9809cc', 'b9d102f3-45aa-4fae-8575-66195e52b389', 'RACC', 1.0, 15.0, ''),
('e0336ca6-fdc1-4797-83a0-6001daf51350', 'fbb999bc-9eb7-4571-b101-93e8fb10cfe2', 'RACC', 1.0, 30.0, ''),
('eed2e7de-7116-4ea5-8a6e-8dd6d6448686', '8f084c85-0366-4b33-ae96-50596c4845fb', 'RACC', 1.0, 85.0, ''),
('0ad9ff81-1f0b-42dc-b7af-3abfc5dbcf9a', '7482ae17-5953-45b7-a84b-398bce7a7686', 'RACC', 1.0, 15.0, ''),
('13006bfc-5a9c-4001-9b0e-c109588e1046', 'b9d214ed-fbc3-417c-bc6a-1551e72f9977', 'RACC', 1.0, 110.0, ''),
('e0d0da20-1213-4c86-829d-780ca3c2086b', '5889ce07-b4b8-4bff-8cc7-07118c5742d1', 'RACC', 1.0, 30.0, ''),
('c673c1ed-ae89-4e5d-b5e6-4ce43d5cdbd7', '978bf4b7-91e2-48b1-9ca9-980b502b7160', 'RACC', 1.0, 15.0, ''),
('33dc7b75-e678-409f-bb04-66afe160c336', '3caeb871-ecf0-4b2c-a52b-83f18e22a25f', 'RACC', 1.0, 30.0, ''),
('0aed7fc4-b545-4f1c-9f4f-075a5753366a', '33f4f503-ec53-4158-a403-0f9c650dd42a', 'RACC', 1.0, 85.0, ''),
('adb93875-dbfd-454f-ac53-6eca80cd872c', 'f2180848-fae5-4f6a-b42d-2906ab64a8be', 'RACC', 1.0, 85.0, ''),
('f7f241aa-08cf-4a4c-96c1-1e363651dfa3', '13624c6c-1527-48bc-9e0f-4c22b106d0e0', 'RACC', 1.0, 85.0, ''),
('5a37a78a-14c3-4b6f-8c33-76d605829413', '4fa5e262-56f5-4b97-9a5f-935668114a0c', 'RACC', 1.0, 85.0, ''),
('70b5f4bc-b046-4911-97cf-c44178ebd6ce', 'da220231-8c0a-4315-9b63-4f827b23a1f3', 'RACC', 1.0, 30.0, ''),
('fc7579ed-6bd7-4b62-8226-b85015f0c904', '134a91db-7e62-450b-8151-c74d3613e8fb', 'RACC', 1.0, 30.0, ''),
('3cfba59f-a9dd-4040-95cb-a42467ac9449', '14348c16-30d1-4c83-8c21-71fdd434c142', 'RACC', 1.0, 30.0, ''),
('f337db48-6cfc-47a2-90cc-4faf52022fcb', '96c298da-fb3a-4da0-b364-0929e3c8c05a', 'RACC', 1.0, 30.0, ''),
('352c8298-5fee-436d-9e66-1c26c6b79422', '0116eaf1-7b65-4ef7-b22d-afb778e1aeaa', 'RACC', 1.0, 40.0, ''),
('efb2c2e9-3040-4b42-bd19-2192555fbe41', 'cb665086-eb83-49e2-99be-ac2bd7443e65', 'RACC', 1.0, 40.0, ''),
('6e56ab17-9ea4-417a-b9ce-9d06da2aac41', 'c465be7f-6ae9-4691-aa30-daf8df951512', 'RACC', 1.0, 140.0, ''),
('0633d388-b0d8-49dd-a334-52c0006a069b', 'b367915d-de7e-4d44-ae90-6d3fb25c3895', 'RACC', 1.0, 140.0, ''),
('e5386481-31cd-4ef4-ac2d-75c158377011', '5c348abe-7ebd-402a-9449-f8e46a0eac17', 'RACC', 1.0, 85.0, ''),
('4daccf06-9293-4a14-952d-6cbee6ca4484', 'cda1f44d-55a4-432b-840d-f0fedac9eaf6', 'RACC', 1.0, 110.0, ''),
('abbc1470-f028-4ab6-b7ee-87d0e6e19d34', 'ad237d9f-a9b9-43ab-a2aa-26c3196445c2', 'RACC', 1.0, 110.0, ''),
('8276016f-0bf7-4fcf-98ec-bc2abf42c39c', '381eccf8-ab3c-4663-8191-d669649162ef', 'RACC', 1.0, 110.0, ''),
('04de71bc-909c-417c-a4f5-16e94d0f1574', 'e265b908-0788-4d93-8734-944a004c44f8', 'RACC', 1.0, 110.0, ''),
('2381511e-356a-4605-895a-afe7492918c5', '8c9dfbdc-f795-478d-aeb2-3e255bd9458d', 'RACC', 1.0, 85.0, ''),
('b3cdb0b6-d337-40f1-b58d-9b840309e09f', '962d5945-8ef4-4af8-a55b-2a495ee029be', 'RACC', 1.0, 85.0, ''),
('9cfa47a4-9d87-4256-a1fe-f0a759cc15a8', 'c07a2527-a263-4ec3-b8f3-cdc00eef4624', 'RACC', 1.0, 85.0, ''),
('e448ca6a-5d12-417e-acdd-40065c8bd06c', 'ac731a0f-0419-4d2e-8ffb-bbd0c483f764', 'RACC', 1.0, 85.0, ''),
('20143c98-1464-45de-ac9d-0e5e17f697d5', '994433da-1e94-4fee-adaf-d8292388d885', 'RACC', 1.0, 140.0, ''),
('1f923b25-a728-42eb-a147-3c0d1a98fa11', '5e77c434-bc1a-4a69-abd0-36544b39e823', 'RACC', 1.0, 140.0, ''),
('661cf444-4a35-41b2-8bb2-14eedddd7503', 'e53e2334-0e0d-46c0-9514-dfa3f744958e', 'RACC', 1.0, 140.0, ''),
('8b1e06eb-cc3c-440c-817f-1b74b9797214', 'ccb4eed9-cc05-477a-a656-263cff0033bc', 'RACC', 1.0, 140.0, ''),
('a3b222a6-5648-49b2-840b-363cb72d8f93', 'a0ba429b-ec5b-4266-b61c-b5275207cfcf', 'RACC', 1.0, 140.0, ''),
('c4fcb0a1-a678-4125-950a-cdab3fa5631d', '57ef41d7-1c58-4043-a45d-d545bbb6af58', 'RACC', 1.0, 140.0, ''),
('ba2004b8-0dba-4866-a844-fed6fc930b1a', '5a0e11aa-90a8-4235-8493-b268f0dfb54f', 'RACC', 1.0, 30.0, ''),
('c5cc7f5a-0a23-41c4-b33e-38c6297d8374', 'bdda4ef8-086c-44a3-a85e-655479313b36', 'RACC', 1.0, 30.0, ''),
('3c71afd6-57ae-4c06-af6a-55dfd82f2229', '099b2be3-2071-4d33-92a3-e169b236751c', 'RACC', 1.0, 45.0, ''),
('7a9b99f3-c63c-4250-adc3-200fa60d093c', 'd55f35a2-cc1b-42ea-92a8-b79d0b5bb010', 'RACC', 1.0, 30.0, ''),
('08476af6-8ed3-43aa-a509-b615b7eda0e6', '4e4f7595-1ba8-4b44-9b03-dcaeea75783c', 'RACC', 1.0, 30.0, ''),
('af9698a8-7753-4e31-a843-92d31e33a108', '65e94706-ce59-4b0f-b812-23626a66b165', 'RACC', 1.0, 30.0, ''),
('d1d9335a-937f-48c2-9984-904a779544bb', 'c24b23fc-3fab-4f7c-9ce6-1226d79bcf8f', 'RACC', 1.0, 30.0, ''),
('e600c7ba-8452-4e0a-ab78-f25febeaa80a', '1d000fea-31d7-4a12-8e6d-26045413b926', 'RACC', 1.0, 45.0, ''),
('698f4b13-c509-4ccf-bbc3-409014d64b5b', '1ad01d7b-c8f5-4ea1-9b10-fa7eccfb5a22', 'RACC', 1.0, 45.0, ''),
('b17690fb-f37f-4918-9ff9-d7d24162ad37', 'fa7df977-04f5-4b4c-8e1b-8117e2c6474c', 'RACC', 1.0, 114.0, ''),
('71fd6ad5-69e5-47ef-be20-9799248dfcc4', '298b0857-7e45-4c84-b3a7-8c3a471ab846', 'RACC', 1.0, 114.0, ''),
('1b504b7f-3b59-4953-8745-1f4f2ea71e82', '1dce7ea4-5ff0-4d98-9ad2-d17e88568df8', 'RACC', 1.0, 110.0, ''),
('5b3e4857-0288-4483-a41b-97d860b1f1f8', '22c8bb88-006b-4d55-a4a3-b8ef552412a7', 'RACC', 1.0, 114.0, ''),
('58e7daf4-75c6-43e9-b706-0c289949b648', '7c623026-507c-4d69-8d37-d13676b9b7d1', 'RACC', 1.0, 114.0, ''),
('020709c9-5bff-4751-a8d9-063e757c1713', 'a7d146be-756b-40d7-8127-b4bc166e4dfd', 'RACC', 1.0, 30.0, ''),
('d99f8053-484d-4681-a237-0a0bf7feed99', '58fb5237-eb80-42aa-9439-5026e16233d9', 'RACC', 1.0, 30.0, ''),
('812d8cfb-597f-41ce-bf0a-4b28aadc3cc1', 'c16bcb67-b579-42b2-a4b5-c05fe157cefa', 'RACC', 1.0, 30.0, ''),
('6c3f56b5-28eb-4e71-83af-782b6e548e00', 'fdea30bf-cdf4-40cd-b695-a22905a64569', 'RACC', 1.0, 30.0, ''),
('65c82ac4-b7a6-4520-baf8-8bef5f776eb9', 'e477ae48-4779-4b1a-bcc6-b5ac8418e0b0', 'RACC', 1.0, 30.0, ''),
('a32dc0ce-c6ab-4612-9816-bf2519083ca0', 'b73e358b-472d-4763-a650-fdfc66a679c9', 'RACC', 1.0, 30.0, ''),
('e40ff848-3045-4956-8ba0-57d17fc0a075', '95652df6-0d7a-4317-896a-5247bc308280', 'RACC', 1.0, 30.0, ''),
('456806a3-9ac9-40cb-aff5-07d0ff1dfe1f', 'b7bd37bd-4c54-4729-9a65-d8ed881c7a78', 'RACC', 1.0, 30.0, ''),
('3051a7f3-0cff-47ae-8f45-7679a6ad3ad2', '41cb1d20-9899-4ab8-84ef-2ca1a8e9a579', 'RACC', 1.0, 30.0, ''),
('82815fe6-2625-4bb7-88c2-13419cb461ed', '67c43c6f-e4ec-4b7b-8830-9524e7b2b2c5', 'RACC', 1.0, 15.0, ''),
('c03686b6-5fd8-4c85-81ac-b8d03e956733', '42da2d27-c405-4fdc-b848-79f9d5b330d3', 'RACC', 1.0, 35.0, ''),
('27d671df-082f-4ed7-ad64-c94c26accd2c', '7dc9bfb7-bc2c-4a35-ac51-4225f38237d8', 'RACC', 1.0, 35.0, ''),
('41109a67-282c-484b-83f2-1e702b9b74b6', 'd0d44605-bd16-42a9-af6e-182fa87b73c8', 'RACC', 1.0, 35.0, ''),
('88ea19e9-0f88-473c-8294-6f737f0d7d56', 'e81ceeae-c530-478f-acbd-54adfa01b23c', 'RACC', 1.0, 35.0, ''),
('d2d09fc9-e8bf-4ed3-97f9-20c87434e82e', 'f5d2ccec-7398-47d7-b3c6-7ff6f720fde2', 'RACC', 1.0, 130.0, ''),
('dbd0a825-b9eb-4d78-aef4-859fde4d23a3', 'ae1e247c-79bd-4a34-8219-e0cfdb7a0938', 'RACC', 1.0, 130.0, ''),
('59f51a8e-1c09-44cf-aa68-30cc68834bc1', '619c2195-618d-4e5e-a399-82de37c31bde', 'RACC', 1.0, 130.0, ''),
('9e2393df-98e9-4651-960b-f5d2822d4882', '11a7f94a-8fc7-4e3c-8293-39d1ca69e1c4', 'RACC', 1.0, 130.0, ''),
('3e035f35-ef34-4df3-a59b-a9df3cfe7f51', '894bda14-c741-4882-8d4b-e6f92a7e7f98', 'RACC', 1.0, 130.0, ''),
('e10bb410-b01d-4e2e-8b9e-b81c068f1c55', 'f8bde1f1-3bec-43ed-a2eb-e97690a701b2', 'RACC', 1.0, 130.0, ''),
('e979807d-13cd-4923-9b2c-287d1731d2f8', '51b3bb9b-823d-4a53-806a-76a51e167730', 'RACC', 1.0, 130.0, ''),
('5032098c-b02e-448f-8a4d-a743a8ceea4d', '150a0326-96d1-433d-85be-39f50d1b2126', 'RACC', 1.0, 130.0, ''),
('cf3b01cb-7e5a-4e23-a0eb-4f458e454cd1', 'f0264207-1519-4d07-b0e4-65a1be20b2a7', 'RACC', 1.0, 130.0, ''),
('065db597-db54-4372-b2e7-1c5053a7d4a4', '4b2c3785-87b9-412a-92d3-2fffac9d01af', 'RACC', 1.0, 130.0, ''),
('625375d3-5721-4911-a224-241b493a160b', '86b00e0c-8679-4274-996a-af7da5f1671b', 'RACC', 1.0, 110.0, ''),
('3137d2f6-8103-4f1c-b12a-06cd45b941fc', 'db7ab9c4-142b-4c33-b84f-9fa60e8b24a5', 'RACC', 1.0, 110.0, ''),
('0982a864-c856-4794-b941-1125675ec55a', '8adbf052-5285-423b-a9b0-9e7fc1e0367b', 'RACC', 1.0, 114.0, ''),
('2c0a751a-55d1-4ff4-81e0-8ffbb35345d0', '449ce785-f355-4d52-9fd2-668fb09541ee', 'RACC', 1.0, 114.0, ''),
('e8e336df-0b07-46ae-aa53-10605d97a263', '6960e381-c8d4-4b09-b6a2-c61ec4be5cf6', 'RACC', 1.0, 114.0, ''),
('96a020cf-886c-4cf6-b1b0-7eff798b17ff', '49985801-1e3b-47cb-a915-7f9553b409e7', 'RACC', 1.0, 114.0, ''),
('dfc87345-aab1-4e5b-ac4c-0ffb472df701', '4c356049-338a-4ae3-8851-ed442c57e694', 'RACC', 1.0, 114.0, ''),
('f233dec8-0818-4048-aa2c-1bbd184e283f', 'c4c10aac-f7cf-4abd-9651-2917401bd0e2', 'RACC', 1.0, 114.0, ''),
('90b2de0f-f7fc-44f7-8f7b-4109bcf272f7', 'e230afa0-ee90-4b37-87c2-778a34f4d5bd', 'RACC', 1.0, 170.0, ''),
('4863f432-4391-4994-96e1-725e21ad0b84', '56e351a1-f3bd-4c6c-aba8-0d3b9a4cc74e', 'RACC', 1.0, 30.0, ''),
('f272a107-b900-4ffe-8b1f-d5be0eb3b19e', '78a454fe-9cb1-4f7a-b797-bc86b939e927', 'RACC', 1.0, 30.0, ''),
('a4071804-0088-4d55-843b-0c023de3e796', '1c5dae43-8d0b-4e6b-8ab6-d25aa974c3bc', 'RACC', 1.0, 30.0, ''),
('780a9543-f90c-4100-9562-b4771e76ed1c', '52ce2c7b-8f13-4dde-963c-25653ee99d82', 'RACC', 1.0, 30.0, ''),
('52672c79-d11c-451d-bdb0-b6d4990ad446', '282d91f1-cdf2-48d3-97ba-c8f2b043049d', 'RACC', 1.0, 30.0, ''),
('f2c172cd-5f1d-4d71-a5c5-bdc584086c3b', '43844bd6-1718-42ed-a8c3-9ece1a69cd66', 'RACC', 1.0, 30.0, ''),
('359d10e2-19ec-4313-a0b6-768284cb834f', '7c1bd87b-82a1-4f76-9a6d-e7ec6f47cb76', 'RACC', 1.0, 115.0, ''),
('753f9a49-08fb-4611-a7f7-2baa3eabecee', 'b0eeacf5-3efe-426a-8504-90ede62e5752', 'RACC', 1.0, 115.0, ''),
('10221c2d-1493-4157-88ff-03389e3a48e5', '4d362779-2a4a-4275-8b4c-f4460cc39ac7', 'RACC', 1.0, 115.0, ''),
('144071ed-406c-451c-8452-2995d395c4d7', '633b30dc-f431-4d19-bff7-04ebfa76126d', 'RACC', 1.0, 115.0, ''),
('ea068193-627e-4400-a5d3-2afea833c377', '57b0e5cc-2d38-4039-b2c2-c06836613db3', 'RACC', 1.0, 115.0, ''),
('a7ec4a35-dfe8-43b0-9071-f26110915afd', '0ba8b17f-6604-448a-a299-41e784802c90', 'RACC', 1.0, 115.0, ''),
('aae3628a-bb9d-49da-b1ce-7defc3cbfeca', '34a1ddf1-8057-4ef9-86f6-9896b1a39757', 'RACC', 1.0, 85.0, ''),
('e792b61f-3a7b-49c3-8c24-cbb23499ef12', '0f66a854-3bdf-49ff-9420-04a67d5d17d4', 'RACC', 1.0, 85.0, ''),
('b797a626-7c42-4951-a592-a64f24e66c05', '6e56c867-d9f8-4502-b3b4-240d813fdc6c', 'RACC', 1.0, 85.0, ''),
('cb6a3623-0653-4f93-a943-cf6525045715', 'd5e84929-27bf-418b-9d60-9b58c09f2334', 'RACC', 1.0, 85.0, ''),
('9d58dea4-4c84-42b6-bd9f-5e56e5bb40dd', '56bf7422-ac21-45a5-9b78-b3b7fde040ea', 'RACC', 1.0, 85.0, ''),
('9abf913d-3530-465f-89d1-a484af1474ec', '714369ae-800f-4802-9fbc-ce4d9bb6d6cf', 'RACC', 1.0, 85.0, ''),
('732a65eb-c58d-41bd-95d3-eb252dd5c17d', '50de6007-8a04-4737-8426-2947c9c35ef7', 'RACC', 1.0, 85.0, ''),
('53e755f8-42cd-45d6-946b-3df54012349c', '94b5707c-5e39-4ba4-b8d1-7529d350882c', 'RACC', 1.0, 85.0, ''),
('c152be0f-3cb9-4a68-864c-17ccc84c6892', '14242608-3f58-4a5f-9642-1ecc43e348ec', 'RACC', 1.0, 85.0, ''),
('2778f15d-206a-4c7a-878b-da7a3a85fc52', 'd1421d9c-c87e-45e0-8145-606a6092ef89', 'RACC', 1.0, 85.0, ''),
('ffb43ce1-a693-4e11-b014-c69bacf9351f', 'a1bba323-8527-41f0-8617-0327f49509f9', 'RACC', 1.0, 85.0, ''),
('35011f55-4672-4d48-8a1c-3fe9141da65e', 'cbad6251-2bbe-4987-8927-e2484360dffe', 'RACC', 1.0, 130.0, ''),
('41dc31cd-8b4c-4bd4-9fde-eee889567ae0', '926217f2-cb16-4af5-8668-fe3f0156377f', 'RACC', 1.0, 130.0, ''),
('35a620bb-4e87-4a01-831a-6deb68905069', 'ff16ef9f-c8eb-47af-b1bb-acaf0c06edc5', 'RACC', 1.0, 30.0, ''),
('0019f269-6a41-42b1-bc94-ab23d62ec353', '8a2ba283-954b-43a5-8711-100b14a7da01', 'RACC', 1.0, 130.0, ''),
('85a4868a-42b8-41e7-996f-2a84ed4e0625', 'd0a7ba8d-0f97-47b3-a859-ab3632321c39', 'RACC', 1.0, 130.0, ''),
('de85101e-ad1f-49dd-b1da-086caa2b5b92', 'f1f51201-d189-43eb-9c26-aa700b28cca9', 'RACC', 1.0, 140.0, ''),
('b6a059c0-0fd7-4056-ae6b-ac3eac48ae63', '2429581d-28c5-4fde-9830-8d96978bebaa', 'RACC', 1.0, 140.0, ''),
('d47469ca-a853-4925-95d4-9d1a0b099d44', '25866128-6503-42fb-af1b-4804d9d87612', 'RACC', 1.0, 140.0, ''),
('cff14fb7-1a2a-4fa3-97e5-843336e478b8', '55a3b1fa-0b5b-4a23-b16d-a46e296a2225', 'RACC', 1.0, 140.0, ''),
('c78df0a4-69fa-4f02-b86b-a386608fd03a', '4f45504e-3d3c-4ab5-a27f-a48bd62cda95', 'RACC', 1.0, 45.0, ''),
('645b4e24-de19-4131-9b7a-7bf7e144a511', '422f17ac-6b43-4293-844d-32de3f9901d6', 'RACC', 1.0, 45.0, ''),
('34c0f18d-885e-407e-b904-dfd9a9d24c89', 'b562eb81-047d-4341-a14f-c0d047cefc1e', 'RACC', 1.0, 45.0, ''),
('67ba509d-ab21-422c-b8ff-93c32f8c91f6', '4bfbca66-59b7-4541-afe4-7ca98ab54e6f', 'RACC', 1.0, 85.0, ''),
('192428cc-92f9-4526-a602-7409da40c4d0', 'e5ab4dd5-902c-4c34-986f-332611b70035', 'RACC', 1.0, 85.0, ''),
('12d5bdde-13f9-4658-974f-8ef99b92bdfb', '86dd1f19-2e60-49d3-a184-adc24e02fd0c', 'RACC', 1.0, 140.0, ''),
('c87495d7-3c97-4bce-9cfe-4b3715ca9417', '2e28fa1d-4a6e-4ba8-a55e-3dec6968f3be', 'RACC', 1.0, 45.0, ''),
('3edb82f1-08b0-4608-9f9d-59fa484d866c', '5ccbc08b-67cf-4d0b-beb8-3cb238c4269d', 'RACC', 1.0, 85.0, ''),
('643199bd-c391-4df5-baea-2c61116d0910', '247cb0d4-bc2d-4b28-9690-c18b940c83d2', 'RACC', 1.0, 45.0, ''),
('fa5bd1e9-2a3a-4146-981d-56d9270708fc', '15f9d632-24ec-41e3-a5c7-2518d6a84766', 'RACC', 1.0, 45.0, ''),
('d28669f3-c588-4c94-8376-4f9e6bd24d3a', '5c52f24d-d242-4382-ab1e-dc5b18b07eb8', 'RACC', 1.0, 45.0, ''),
('0d60c3e2-e4dd-46a5-a979-86f2e6c0af47', '361b67a2-5e54-4010-8af8-12b3769c6466', 'RACC', 1.0, 45.0, ''),
('9939e502-ccbd-453a-b73b-e5cc4b543b0b', '78c028ab-e626-47ac-a64d-28e6fe4bcecf', 'RACC', 1.0, 140.0, ''),
('919015ec-863f-45f3-a6b7-a447ab7543d3', 'b6a54cb6-2b22-4048-8c18-bbe1d3ae197c', 'RACC', 1.0, 140.0, ''),
('4cc4f5fd-dd43-4e45-b179-df1143c096e4', '4fdd2de5-2a1b-4469-8b73-f04e6dd5f1aa', 'RACC', 1.0, 140.0, ''),
('7063fe66-36f0-4057-ab1f-382f18ed763f', '259ce4fd-7bd6-4b9c-b781-cc215d7be6ed', 'RACC', 1.0, 140.0, ''),
('eca54392-43d0-4309-8354-dc51b32d5360', '043f1148-2287-4377-8026-7390f5f8f095', 'RACC', 1.0, 30.0, ''),
('1a4836f8-312c-43f0-92ee-86f1d5d64c5c', '050f8308-1e5c-4839-b19e-86968e65b4b2', 'RACC', 1.0, 140.0, ''),
('95c24622-893d-44c9-8dd8-ee0e3292bc59', '28c16b72-a83f-4c49-a647-7903ebf28017', 'RACC', 1.0, 140.0, ''),
('f73b7871-43c8-4d3e-adcf-9120ece3f488', 'eaea2c3d-5e58-425c-a7c2-185083a4d343', 'RACC', 1.0, 45.0, ''),
('5cf696ee-2f37-441e-a5c0-a63fde306ae4', 'a6edd591-f6bb-4706-9811-e2ccc0f5f490', 'RACC', 1.0, 45.0, ''),
('acc8deb0-4c4c-4ffc-a2cc-c968790093ae', 'dc003ed4-6578-4bc3-a5f2-91108286c2f3', 'RACC', 1.0, 30.0, ''),
('71263235-1d64-49e9-b4d4-2ec1ea7ba7a9', '2421ae45-5d9f-4a91-b83a-1d166e1d32fb', 'RACC', 1.0, 45.0, ''),
('5a139eb6-d112-44e4-a810-c12d41573366', '0c2e333d-9830-418b-83bd-bbc30360edc3', 'RACC', 1.0, 45.0, ''),
('55460806-21d1-4950-b08a-c98de779d7a9', 'ac489ae9-9d55-4137-b1a2-5ea94b2cbc61', 'RACC', 1.0, 140.0, ''),
('b366fd2f-42a7-481c-a15a-f71c2b3d1213', '1d2f6050-4789-406f-9b9c-66eb86e08e2a', 'RACC', 1.0, 114.0, ''),
('017ff395-1ff6-41ae-a1b8-5d90176853f8', '843820a9-87e2-4dfa-a0a3-a1c62d4bceba', 'RACC', 1.0, 114.0, ''),
('aaffd15b-5a15-49ba-be83-523182be77f9', '82abcf24-3191-4919-847f-d1b9f1393df7', 'RACC', 1.0, 114.0, ''),
('34c823be-dd76-4af1-9505-bd2a9b6cf1ab', '1b145490-c70f-4465-895f-3234f7ef7307', 'RACC', 1.0, 114.0, ''),
('ab52a04f-e3a6-4a82-bccc-d83162a6f27c', '7d6fd4a4-1360-4e10-928d-9ed5b267c4d2', 'RACC', 1.0, 114.0, ''),
('39726106-49ef-47ed-b1bd-caccd9e203a9', 'f15af3b0-50a6-4f69-a007-6c50a3374b23', 'RACC', 1.0, 114.0, ''),
('729b3365-db68-46d5-9162-fc98bc2706e6', '50786aea-565c-4c27-ad9a-88138b99ac8d', 'RACC', 1.0, 114.0, ''),
('7324073d-0ec5-4645-ab3a-55cc14bc2085', 'e796f002-6678-433a-b678-0325b97a5082', 'RACC', 1.0, 114.0, ''),
('0e80aa20-e11b-4b58-9e45-c84a81a404b9', '1dfca014-3c68-458e-b1d5-1c1a55041f86', 'RACC', 1.0, 114.0, ''),
('a6f79ca6-fd1c-410e-8f7e-39ece7af1dc1', 'e8df79d3-119e-458d-9091-57bfdb247739', 'RACC', 1.0, 110.0, ''),
('45603879-8fa6-4150-8e3b-5b2435bfc6c7', 'eed1b75d-e9a6-48ae-a23f-2ba115c50eb3', 'RACC', 1.0, 26.0, ''),
('81748d52-f360-4cef-af44-1db1ce7ebab2', '02467187-75e8-40f5-8194-47d123df0d35', 'RACC', 1.0, 85.0, ''),
('633f746d-2691-4668-8d8d-7a7818672088', '1cb681c6-40f6-4ead-a9b0-c5e5f31c5bf6', 'RACC', 1.0, 85.0, ''),
('920a065c-64f6-466d-989f-8291237d78f8', '71fff9c0-f291-4b9c-9cc6-249e00d95716', 'RACC', 1.0, 85.0, ''),
('039fa929-7869-4a15-8257-041f354b8b0c', 'c939d8af-fe31-45a4-9c14-903a4cfd6388', 'RACC', 1.0, 140.0, ''),
('f1fc1180-8a56-4e79-908b-081b8bfe0974', '7ccb5304-ea69-4fbb-9073-b7123bb9893c', 'RACC', 1.0, 85.0, ''),
('4d5fa5e6-5005-4335-b624-d95cfe5ad179', 'b9a20cdf-7e0d-4f22-9a5d-9a7242d403b9', 'RACC', 1.0, 85.0, ''),
('5277af87-940f-472c-ace2-fdea0443f190', '7a052a1c-6215-4343-aa45-33bdbfeae149', 'RACC', 1.0, 85.0, ''),
('c56c3919-4ade-4be0-90f1-fc0e5eeb6f5a', 'a4c3afce-ba42-421d-aea3-129f9aa0c5c3', 'RACC', 1.0, 85.0, ''),
('92ae9116-8754-44d1-a437-be08b9976d00', 'bf7bf319-ee38-4380-98fc-35fc16793480', 'RACC', 1.0, 85.0, '')
ON CONFLICT (id) DO NOTHING;

-- Migrando 2 Recetas base...
INSERT INTO public.recipes (id, user_id, title, description, servings, final_image_path) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', NULL, 'Ensalada Vital Mediterranea', 'Plato equilibrado rico en potasio, fosforo y proteinas de alto valor biologico, ideal para almuerzos nutritivos.', 2, 'dotnet_bot.png'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', NULL, 'Bowl Calido Energetico', 'Preparacion reconstituyente con aporte sostenido de energia, potasio y hierro para recuperacion muscular.', 2, 'dotnet_bot.png')
ON CONFLICT (id) DO NOTHING;

-- Migrando 5 Ingredientes de recetas...
INSERT INTO public.recipe_ingredients (id, recipe_id, food_item_id, food_name, grams, calculated_calories, calculated_minerals_json) VALUES
('67bc8f75-554d-404d-b32e-355b67363c6d', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '267e2083-92c9-4d25-84f2-736a3f7d44b8', 'Fish, haddock, raw', 150.0, 111.0, '[{"Type":1,"Milligrams":340.5},{"Type":2,"Milligrams":429},{"Type":3,"Milligrams":319.5},{"Type":4,"Milligrams":16.5},{"Type":5,"Milligrams":31.650000000000002},{"Type":6,"Milligrams":0.255},{"Type":7,"Milligrams":0.48}]'),
('d5634027-1443-4ba0-9640-ff5a49e89024', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '2d3c605f-9d78-4d5a-87ba-967ac7f1fa1f', 'Beans, snap, green, canned, regular pack, drained solids', 100.0, 21.0, '[{"Type":1,"Milligrams":23},{"Type":2,"Milligrams":97},{"Type":3,"Milligrams":282},{"Type":4,"Milligrams":36},{"Type":5,"Milligrams":12.7},{"Type":6,"Milligrams":0.78},{"Type":7,"Milligrams":0.19}]'),
('2cce7c4b-8aa4-4923-b12e-551146472206', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'a83e811b-4aef-4e16-bb4d-ffa0b27d0961', 'Cheese, parmesan, grated', 30.0, 126.3, '[{"Type":1,"Milligrams":190.2},{"Type":2,"Milligrams":55.199999999999996},{"Type":3,"Milligrams":525},{"Type":4,"Milligrams":265.2},{"Type":5,"Milligrams":10.469999999999999},{"Type":6,"Milligrams":0.135},{"Type":7,"Milligrams":1.299}]'),
('cf6c81df-4a64-40e6-8c33-af409526906f', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '86bc73c0-8966-4d0a-91f2-6dec9aaeeae1', 'Frankfurter, beef, unheated', 200.0, 628.0, '[{"Type":1,"Milligrams":256},{"Type":2,"Milligrams":686},{"Type":3,"Milligrams":1744},{"Type":4,"Milligrams":30},{"Type":5,"Milligrams":23},{"Type":6,"Milligrams":2.28},{"Type":7,"Milligrams":4.12}]'),
('d2717d5a-125e-4e96-9ef1-71868d0fd7d3', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'fbc06787-3027-4be9-9b56-f27589223534', 'Tomatoes, grape, raw', 150.0, 40.5, '[{"Type":1,"Milligrams":42},{"Type":2,"Milligrams":390},{"Type":3,"Milligrams":9},{"Type":4,"Milligrams":16.5},{"Type":5,"Milligrams":17.85},{"Type":6,"Milligrams":0.495},{"Type":7,"Milligrams":0.30000000000000004}]')
ON CONFLICT (id) DO NOTHING;

-- Migrando 6 Pasos de recetas...
INSERT INTO public.recipe_steps (id, recipe_id, step_number, instruction, image_path) VALUES
('275ae1e1-ec10-44cb-bc30-01a4e081b7ef', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 1, 'Lavar meticulosamente los ingredientes frescos y secar con centrifugadora o papel absorbente.', 'dotnet_bot.png'),
('25d6a5a9-3eed-4734-a021-2901fcaa3112', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 2, 'Cocinar a fuego medio en plancha antiadherente durante 3 minutos por lado hasta lograr un dorado uniforme.', 'dotnet_bot.png'),
('a393522b-80cd-4111-8f65-e485d2263bd8', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 3, 'Disponer los vegetales como base, incorporar la proteina troceada y coronar con las lascas de queso.', 'dotnet_bot.png'),
('dbad90b7-6278-40ef-bf9e-7a7906a388a5', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 1, 'Hervir o cocinar los vegetales al vapor hasta que queden tiernos pero firmes y cortar en dados regulares.', 'dotnet_bot.png'),
('cf978853-e650-4956-8783-187b82d389fc', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 2, 'Trocear la proteina principal y saltear ligeramente junto con el resto de ingredientes para integrar sabores.', 'dotnet_bot.png'),
('aed42391-effc-4410-befc-c33a3609a5cd', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 3, 'Servir en cuencos individuales colocando los vegetales en la base y el salteado en la superficie.', 'dotnet_bot.png')
ON CONFLICT (id) DO NOTHING;
