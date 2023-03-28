# World Builder Sample Scene

This a sample scene for the "World Builder" feature. This feature is still in experimental status, so use it with care!

The basic idea of the "World Builder" is that you can add, place and adjust objects at runtime, and save
the session so that it persists when the app is closed.

This can be used to provide final users to create persistent experiences, or for developers to create experiences
by manipulating objects "on-site", saving the resulting data to be loaded when the final user runs the app.

In this sample, each time the app is closed, the World Session is serialized and saved on local storage; and
each time opens, it loads the saved session and restores it. This is regulated by the "UseLocalStorege" option
in the "WorldBuilder" component. If this is enabled, the session will be saved in local storage to a file named $"{id}.json".
If you disable this option the session won't be saved or loaded automatically. In this case, you can use the `ToJson` and `FromJson` methods
to serialize/deserialize the "World Session" manually. That way you can, for instance, save the session on a server via network requests,
allowing for persistent world sessions in the cloud.

The "World Builder Application Controller" is an implementation of a simple application that uses the World Builder to
position and adjust objects. It can serve as a base for you to modify and customize it to your needs.

This controller has a very simple UI. In the main view, you choose which one of the 3 prefabs you want to place in the world. To place them,
just touch the screen on the location.

After an object is placed, you can select it by touching the object. That will change the UI, which will now display buttons for object adjustment;
you can adjust the y-axis rotation by dragging horizontally, move the object to another location by touching the screen, or change its height by
dragging vertically.
