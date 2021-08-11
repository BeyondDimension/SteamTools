package net.steampp.app.ui.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.LiveData
import net.steampp.app.ui.MainApplication
import net.steampp.app.design.R
import java.util.*

class MyViewModel : ViewModel() {
    private val mText: MutableLiveData<String>
    val text: LiveData<String>
        get() = mText

    init {
        mText = MutableLiveData()
        mText.setValue(
            """
    ${
                Arrays.stream(this.javaClass.name.split("\\.").toTypedArray())
                    .reduce { first: String?, second: String -> second }.orElse("")
                    .replace("ViewModel", "")
            }
    ${MainApplication.context?.getString(R.string.under_construction)}
    """.trimIndent()
        )
    }
}
