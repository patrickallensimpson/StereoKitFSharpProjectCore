open StereoKit
open System

[<Struct>]
type Entity = {
    mutable pose : Pose
    model : Model
    color : Color
}

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"

let settings = SKSettings(
        appName = "SteroKitFSharpProjectCore",
        assetsFolder = "Assets"
    )

if SK.Initialize(settings) |> not then
    Environment.Exit(1)

(*
let mutable cubePose = new Pose(0.f,0.f,-0.5f,Quat.Identity)
let cube = Model.FromMesh(
        Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
        Default.MaterialUI
    )
*)
let cube = {
    pose = Pose(0.f,0.f,-0.5f,Quat.Identity)
    model = Model.FromMesh(
        Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
        Default.MaterialUI
    )
    color = Color.White
}

let mutable entities : Entity[] = Array.singleton cube;

//let mutable windowPose = Pose(-0.4f,0f,0f,Quat.LookDir(1f,0f,1f))
let mutable windowPose = Pose(0f,0f,-0.50f,Quat.LookDir(1f,0f,1f))
let mutable showHeader = true
let mutable slider = 0.5f
let powerSprite = Sprite.FromFile("power.png", SpriteType.Single)
let rand = System.Random()

let displayUI () =
    UI.WindowBegin("Window", &windowPose, Vec2(20f,0f)*U.cm, if showHeader then UIWin.Normal else UIWin.Body)

    UI.Toggle("Show Header", &showHeader) |> ignore

    UI.Label("Slide")
    UI.SameLine()
    UI.HSlider("slider", &slider, 0f, 1f, 0.2f, 72f*U.mm) |> ignore

    if UI.Button("Add new") then
        let depth = -(0.3f + (rand.NextSingle() * 0.4f))
        let vertical = 0.2f - (rand.NextSingle() * 0.4f)
        let horizontal = 0.2f - (rand.NextSingle() * 0.4f)
        let color =
            let hue = (15f * (float32 (rand.Next(24))))/360f
            printfn "hue: %f" hue
            Color.HSV(hue,0.5f,0.8f).ToLinear()
        printfn "color: %A" (color.r,color.g,color.b)
        let newCube = {
            pose = Pose(horizontal,vertical,depth,Quat.Identity)
            model = Model.FromMesh(
                Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                Default.MaterialUI
            ) 
            color = color
        }
        entities <- Array.insertAt (entities.Length-1) newCube entities

    if UI.ButtonRound("Exit", powerSprite) then
        SK.Quit()

    UI.WindowEnd()

let floorTransform = Matrix.TS(0f,-1.5f, 0f, new Vec3(30f,0.1f,30f))
let floorMaterial = Material(Shader.FromFile("floor.hlsl"), Transparency = Transparency.Blend)

while
    SK.Step(fun _ ->
        if SK.System.displayType = Display.Opaque then
            Default.MeshCube.Draw(floorMaterial, floorTransform)
        
        //UI.Handle("Cube", &cubePose, cube.Bounds) |> ignore
        //cube.Draw(cubePose.ToMatrix())
        for i = 0 to entities.Length-1 do
            let entity : byref<Entity> = &entities.[i] 
            UI.Handle($"Cube{i}", &entity.pose, entity.model.Bounds) |> ignore
            entity.model.Draw(entity.pose.ToMatrix(),entity.color)

        displayUI()
    ) do
        ()

SK.Shutdown()

