package com.github.wwluo14.nospacetextview

import android.annotation.SuppressLint
import android.content.Context
import androidx.appcompat.widget.AppCompatTextView
import android.graphics.Paint.FontMetricsInt
import android.graphics.Rect
import android.util.AttributeSet

/**
 * @author luowang8
 * @date 2020-01-17 16:36
 * @url https://github.com/wwluo14/NoSpaceTextView/blob/master/app/src/main/java/com/demo/nospacetextview/NoSpaceTextView.java
 */
@SuppressLint("AppCompatCustomView")
class NoSpaceTextView : AppCompatTextView {
    /**
     * 控制measure()方法 刷新测量
     */
    private var refreshMeasure = false

    constructor(context: Context?) : super(context!!) {}
    constructor(context: Context?, attrs: AttributeSet?) : super(
        context!!, attrs
    ) {
    }

    constructor(context: Context?, attrs: AttributeSet?, defStyleAttr: Int) : super(
        context!!, attrs, defStyleAttr
    ) {
    }

    override fun onMeasure(widthMeasureSpec: Int, heightMeasureSpec: Int) {
        super.onMeasure(widthMeasureSpec, heightMeasureSpec)
        removeSpace(widthMeasureSpec, heightMeasureSpec)
    }

    override fun setText(text: CharSequence, type: BufferType) {
        super.setText(text, type)
        // 每次文本内容改变时，需要测量两次，确保计算的高度没有问题
        refreshMeasure = true
    }

    /**
     * 这里处理文本的上下留白问题
     */
    private fun removeSpace(widthspc: Int, heightspc: Int) {
        val rect = Rect()
        val text = linesText[0]
        paint.getTextBounds(text, 0, text!!.length, rect)
        val fontMetricsInt = FontMetricsInt()
        paint.getFontMetricsInt(fontMetricsInt)
        val paddingTop = fontMetricsInt.top - rect.top

        // 设置TextView向上的padding (小于0, 即把TextView文本内容向上移动)
        setPadding(
            leftPaddingOffset,
            paddingTop + topPaddingOffset,
            rightPaddingOffset,
            bottomPaddingOffset
        )
        val endText = linesText[linesText.size - 1]
        paint.getTextBounds(endText, 0, endText!!.length, rect)

        // 再减去最后一行文本的底部空白，得到的就是TextView内容上线贴边的的高度，到达消除文本上下留白的问题
        setMeasuredDimension(
            measuredWidth, measuredHeight - (fontMetricsInt.bottom - rect.bottom)
        )
        if (refreshMeasure) {
            refreshMeasure = false
            measure(widthspc, heightspc)
        }
    }//指定行的内容

    /**
     * 获取每一行的文本内容
     */
    private val linesText: Array<String?>
        private get() {
            var start = 0
            val texts = arrayOfNulls<String>(lineCount)
            val text = text.toString()
            val layout = layout
            for (i in 0 until lineCount) {
                val end = layout.getLineEnd(i)
                val line = text.substring(start, end) //指定行的内容
                start = end
                texts[i] = line
            }
            return texts
        }
}
