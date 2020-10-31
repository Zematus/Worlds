# Function Expressions Guide

Following is a a list of all function expressions currently supported within the
game:

- **lerp(*start*, *end*, *factor*)**

  This function returns a **numeric** value interpolated between a *start* value and an *end* value using an interpolation *factor* where an interpolation factor of `0` makes the function return the *start* value, and and interpolation factor of `1` makes the function return the *end* value.

  *Parameters:*
    *start:* a **numeric** expression that gives the initial value
    *end:* a **numeric** expression that gives the end value
    *factor:* a **numeric** expression that gives the interpolation factor

  *Examples:*
  `"lerp(1, 2, 0.4)"` returns `1.4`
  `"lerp(4, 2, 0.5)"` returns `3`
  `"lerp(3.5, 4.2, 0)"` returns `3.5`
  `"lerp(5.2, 7.1, 1)"` returns `7.1`


- **saturation(*input*, *half_saturation_level*)**

  This function returns a **numeric** value between `0` and `1` depending on how *unsaturated* or *oversaturated* the *input* value is. Where an input of `0` returns `0`, an input equals to *half_saturation_level* returns `0.5`. And an input above *half_saturation_level* will return an output value closer to `1` as the input approaches positive infinity.

  *Parameters:*
    *input:* the **numeric** expression to test saturation against
    *half_saturation_level:* a **numeric** expression that gives the hypothetical halfway level of saturation

  *Examples:*
   `"saturation(0, 100)"` returns `0`
   `"saturation(50, 100)`" returns `0.33`
   `"saturation(100, 100)"` returns `0.5`
   `"saturation(10000, 100)"` returns `0.99`


- **normalize(*input*, *min*, *max*)**

  This function will take the *input* value and scale it according to the *min* and *max* values, where an input equal to *min* will return `0` and an input equal to *max* will return `1`.

  **NOTE:** *min* must be less than *max*. Otherwise an error will occur.

  *Parameters*
    *input:* a **numeric** expression that gives the value to scale
    *min:* the **numeric** expression that returns the lower end of the scale
    *max:* the **numeric** expression that returns the higher end of the scale

  *Examples:*
  `"normalize(200, 100, 200)"` returns `1`
  `"normalize(20, 20, 50)"` returns `0`
  `"normalize(3, 1, 5)"` returns `0.5`
  `"normalize(10, 20, 40)"` returns `-0.5`
  `"normalize(50, 20, 40)"` returns `1.5`


- **random(*max*), random(*min*, *max*)**

  This function will return a random non-integer **numeric** value between the range of *min* and *max*. *random* has two versions, one that receives only the *max* parameter, and the other that receives both *min* and *max* parameters. If only the max parameter is given, this function will default the min value to `0`.

  **NOTE:** *min* must be less than *max*. Otherwise an error will occur.

  *Parameters:*
    *min:* the **numeric** expression that returns the minimum possible value that can be returned
    *max:* the **numeric** expression that returns the maximum possible value that can be returned

  *Examples:*
  `"random(10)"` returns a random non-integer value between `0` and `10`
  `"random(5, 15)"` returns a random non-integer value between `5` and `15`
  `"random(-2, 2)"` returns a random non-integer value between `-2` and `2`


- **min(*input_1*, *input_2*, *...*)**
  This function will take two or more **numeric** input values and return the lesser of them all.

  *Parameters:*
    *input_<x>:* a **numeric** expression that returns an input value, this function receives two or more of these

  *Examples:*
  `"min(2, 1, 3)"` returns `1`
  `"min(53.1, 34.3)"` returns `34.3`


- **max(*input_1*, *input_2*, *...*)**
  This function will take two or more **numeric** input values and return the greater of them all.

  *Parameters:*
    *input_<x>:* a **numeric** expression that returns an input value, this function receives two or more of these

  *Examples:*
  `"max(2, 1, 3)"` returns `3`
  `"max(53.1, 34.3)"` returns `53.1`


- **clamp(*input*, *min*, *max*)**
  This function will take a **numeric** *input* and an *min* and *max* values. It will return *input* if the value falls between the *min* and *max* values. Otherwise it will return *min* if *input* falls below the given range, or *max* if it falls above that same range.

  *Parameters:*
    *input:* the **numeric** expression that returns the value to clamp
    *min:* the **numeric** expression that returns the minimum value to use
    *max:* the **numeric** expression that returns the maximum value to use

  *Examples:*
  `"clamp(2, 1, 3)"` returns `2`
  `"clamp(-2, 2.3, 4.1)"` returns `2.3`
  `"clamp(6, 2.3, 4.1)"` returns `4.1`


- **percent(*input*)**
  This function converts the *input* value into a **string** percentage representation of that value. This function is mostly useful for dialog texts.

  *Parameters:*
    *input:* a **numeric** expression that gives the value to convert

  *Examples:*
  `"percent(0.5)"` returns `"50 %"`
  `"percent(0.63)"` returns `"63 %"`
  `"percent(0)"` returns `"0 %"`
  `"percent(1)"` returns `"100 %"`
