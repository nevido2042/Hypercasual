# Cash UI Display Implementation Plan

The goal is to display the total value of cash items carried by the player (count * 5) on the "Cash Panel" in the UI.

## Proposed Changes

### [Gameplay]

#### [MODIFY] [PlayerStack.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/PlayerStack.cs)
- Add a public property `MoneyCount` that returns `moneyStack.Count`.
- Add an event `System.Action OnMoneyStackChanged` that triggers whenever items are added or removed from the money stack.

#### [NEW] [CashUI.cs](file:///d:/Fork_Git/Hypercasual/Hypercasual/Assets/01.Scripts/Gameplay/CashUI.cs)
- Create a new script to manage the cash display.
- It will hold a reference to `TextMeshProUGUI` and the `PlayerStack`.
- It will update the text to `(PlayerStack.MoneyCount * 5).ToString()` whenever the stack changes.

## Verification Plan

### Automated Tests
- None at this stage.

### Manual Verification
- Pick up cash items in-game and verify that the number on the Cash Panel increases by 5 for each item.
- Drop off or consume cash items and verify that the number decreases correctly.
