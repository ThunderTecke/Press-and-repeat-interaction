# Press and repeat interaction

This interaction allows you to generate Input System events to manage a held key.
"Press" event can be activate to also generate an event on pressing the binding.

This interaction was developed with **Unity 2021.3.10f1** and **Input System 1.4.4**

## How to install

You can copy this script everywhere in your project folder. But preferably correctly oragnized.
For exemple, in ``Interactions`` folder.

## How to use

You just have to add the interaction on your action (or binding) with the ``+`` and select ``Press And Repeat``.
Then you have to adjust parameters on your convenience.

### Parameters

- ``Press`` : Used to activate ``press`` event. It's send on pressing. Just like normal action.
- ``Hold Time`` : Used to consider binding held, and send a second event.
- ``Repeating Time`` : Used to send next events with a constant time interval.
