# Marker placement prototype

AR_Scene now uses AR Tracked Image Manager and TrackedImagePlacement. The previous
surface spawner components remain in the scene but are disabled. The template UI
is inactive because it manages manual placement. A separate Marker Placement UI
canvas contains the Choose character button and the reused template object menu.
MarkerCharacterMenu handles opening/cancelling and closes the menu on tracking loss.
The choices call TrackedImagePlacement.SpawnCharacter with indices 0 through 3.
Character tap interaction remains
on each character prefab and uses the existing screen-space XR interactor.

The entire marker-001.png is the reference image. Its physical width is set to 0.07 metres, matching the measured 7 x 7 cm display
on the test phone. Keep that display size fixed; update MarkerReferenceLibrary
if the full image is displayed at another size. The image library currently
contains a single image named marker-001. QR payload decoding is not implemented.

TrackedImagePlacement shows a cyan placement indicator at the QR centre and a
Choose character button while the marker is tracked. Choosing Pikachu, Charmander, Squirtle, or Bulbasaur from the template object menu creates one character
and hides the indicator. The button remains available while tracking so another choice
can replace the character at the same marker position. Choosing the current character
only closes the menu. It aligns the character with the QR centre, hides
it when tracking is not Tracking, and reuses it after reacquisition. Placement Point
is normalized from the top-left of the full image. Character Scale multiplies the
prefab's scale; the final size depends on the selected prefab. Surface
Offset and Rotation Offset can be adjusted on Object Spawner in AR_Scene.

## Editor simulation

The selected XR Simulation environment is Assets/Simulation/MarkerSimulationEnvironment.prefab,
a local copy of the package environment. Its existing simulated tracked image uses
marker-001.png and is 7 cm wide, horizontal on the table. The starting camera looks
at it. The environment selection is an Editor preference shared by scenes; no
SampleScene or package prefab was edited.

1. Allow Unity to import and compile, then open/reload AR_Scene.
2. Enter Play Mode. Image detection should show the cyan indicator and Choose character button.
   No character should appear until you choose a character. Losing tracking before choosing
   a character should hide both controls; reacquiring the marker should restore them.
   Open the menu and choose Pikachu, Charmander, Squirtle, or Bulbasaur: one character should appear and the indicator should disappear.
   Reopen the menu and select another character: it should replace the previous one.
   Selecting the same character should close the menu without restarting it.
3. Navigate with right mouse + WASD and click/release on the character to react.
4. Look away, then return: the character should hide when tracking is lost and
   reappear as the same instance when tracking resumes.
5. To inspect or move the simulated target during Play Mode, expand the simulation
   environment scene in the Hierarchy and find Marker 001 (7 cm).

XR Simulation uses the SimulatedTrackedImage component; it does not recognize the
rendered pixels with a real camera. This validates the placement flow, not Android
image-recognition reliability. Do not add a second environment instance to AR_Scene.

No Android build was run and build settings were not changed.
