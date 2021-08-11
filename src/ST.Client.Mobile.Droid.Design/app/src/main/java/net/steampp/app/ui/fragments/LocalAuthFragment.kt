package net.steampp.app.ui.fragments

import net.steampp.app.ui.viewmodels.LocalAuthViewModel
import android.view.LayoutInflater
import android.view.ViewGroup
import android.os.Bundle
import android.view.View
import androidx.lifecycle.ViewModelProvider
import net.steampp.app.design.R
import android.widget.TextView
import androidx.fragment.app.Fragment

class LocalAuthFragment : Fragment() {
    private var viewModel: LocalAuthViewModel? = null
    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?, savedInstanceState: Bundle?
    ): View? {
        viewModel = ViewModelProvider(this).get(LocalAuthViewModel::class.java)
        val root = inflater.inflate(R.layout.fragment_local_auth_test, container, false)
        val textView = root.findViewById<TextView>(R.id.textView)
        viewModel!!.text.observe(viewLifecycleOwner, { s -> textView.text = s })
        return root
    }
}
