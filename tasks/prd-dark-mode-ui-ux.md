# PRD for Dark Mode and UI/UX Improvements

## Description
The app currently has issues in dark mode due to hardcoded colors and lack of theme adaptation.
We need to fix the UI to work well in both light and dark modes, and improve overall user experience.

## User Stories

### Story 1: Replace hardcoded colors with theme-aware resources
As a user, I want the app to adapt to light and dark mode so that it's readable in both.
- Replace all hardcoded BackgroundColor values (LightYellow, LightBlue, LightGreen) with dynamic resources
- Ensure text colors are readable in both modes by using appropriate dynamic colors
- Update SwipeView delete button to use theme-aware colors
- Check all frames and labels for color issues

### Story 2: Improve Pantry page UI/UX
As a user, I want the pantry page to be more intuitive and visually appealing.
- Make the add ingredient form more compact and visually balanced
- Improve spacing and alignment of elements
- Consider using input validation for quantity field

### Story 3: Improve RecipeDetailPage layout
As a user, I want the recipe detail page to be more readable and easier to use.
- Better organization of ingredients and instructions sections
- Improve the visual hierarchy of information
- Make the cooking instructions more readable

### Story 4: Ensure consistent styling across pages
As a user, I want a consistent look and feel throughout the app.
- Standardize button styles
- Consistent padding and spacing
- Unified typography

## Acceptance Criteria
- All pages display correctly in both light and dark mode
- No hardcoded color values that break in dark mode
- UI improvements enhance usability without removing functionality
- All existing tests continue to pass
