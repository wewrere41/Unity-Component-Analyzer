# Component Analyzer For Unity


### An analyzer that analyze components initialization to prevent the user from using an uninitialized component.


#### If all of the following conditions are true, it generates an warning
* Field accessibility non public
* Field not marked with [SerializeField] attribute
* Field not initialized in user code. (GetComponent<T>,FindObjectsOfType<T> etc)
* The user accessed the field.







https://user-images.githubusercontent.com/45511976/210083542-fa384b6c-2cec-4ab8-9655-8c39727a35b7.mp4

