-- =============================================================================
-- DietApp - Esquema Relacional de Base de Datos para Supabase (PostgreSQL)
-- =============================================================================
-- Ejecuta este script en el SQL Editor de tu proyecto en Supabase (https://supabase.com/dashboard)
-- Crea las tablas con sus relaciones foraneas, indices B-Tree para consultas de minerales,
-- soporte para autenticacion/login de usuarios y politicas de seguridad RLS (Row Level Security).
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- -----------------------------------------------------------------------------
-- 1. Tabla: user_profiles (Perfiles y Preferencias vinculados a auth.users)
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
    is_global BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_recipes_user_id ON public.recipes(user_id);
CREATE INDEX IF NOT EXISTS idx_recipes_is_global ON public.recipes(is_global);

-- -----------------------------------------------------------------------------
-- 6.1 Tabla: user_recipe_favorites (Favoritos de recetas por perfil de usuario)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS public.user_recipe_favorites (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    recipe_id UUID NOT NULL REFERENCES public.recipes(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_user_recipe_favorites UNIQUE (user_id, recipe_id)
);

CREATE INDEX IF NOT EXISTS idx_user_recipe_favorites_user_id ON public.user_recipe_favorites(user_id);
CREATE INDEX IF NOT EXISTS idx_user_recipe_favorites_recipe_id ON public.user_recipe_favorites(recipe_id);

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
ALTER TABLE public.user_recipe_favorites ENABLE ROW LEVEL SECURITY;
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

-- Politicas para foods
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

-- Politicas para recipes, user_recipe_favorites, recipe_ingredients y recipe_steps
CREATE POLICY "recipes_select_public_or_own" ON public.recipes
    FOR SELECT TO anon, authenticated
    USING (is_global = true OR user_id IS NULL OR (select auth.uid()) = user_id);

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

CREATE POLICY "favorites_select_own" ON public.user_recipe_favorites
    FOR SELECT TO authenticated
    USING ((select auth.uid()) = user_id);

CREATE POLICY "favorites_insert_own" ON public.user_recipe_favorites
    FOR INSERT TO authenticated
    WITH CHECK ((select auth.uid()) = user_id);

CREATE POLICY "favorites_delete_own" ON public.user_recipe_favorites
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
