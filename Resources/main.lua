-- Game Object, where the engine hooks the functions
Game = {time = 0}

-- Player Object, intialization
Player = {
    x = 0,
    y = 0,
    texture = -1,
    move_speed = 2
}
function Game.init()
    -- Run once when the game started
    set_game_title("My Game")
    set_window_icon("icon.png")

    -- Load a texture for our player
    Player.texture = load_texture("icon.png")
end
function Game.update(delta)
    -- Run every frame
    Game.time = Game.time + delta

    -- Movement
    move_h = 0
    move_v = 0

    -- Check if keyboard is pressed
    if is_key_pressed(keycode_W) then
        move_v = move_h - 1
    end

    if is_key_pressed(keycode_S) then
        move_v = move_h + 1
    end

    if is_key_pressed(keycode_A) then
        move_h = move_h - 1
    end

    if is_key_pressed(keycode_D) then
        move_h = move_h + 1
    end

    -- Multiply by delta so it moves constantly independent to framerate
    move_h = move_h * delta * Player.move_speed
    move_v = move_v * delta * Player.move_speed

    -- Apply the movement
    Player.x = (Player.x + move_h)
    Player.y = (Player.y + move_v)
end

function Game.draw()
    -- Run only when need redraw
    draw_texture(Player.texture, Player.x, Player.y, 32, 32)
end

function Game.dispose()
    -- Run before everything closes
end

function Game.resize(w, h)
    -- When the window is resized
    print(w, h)
end
