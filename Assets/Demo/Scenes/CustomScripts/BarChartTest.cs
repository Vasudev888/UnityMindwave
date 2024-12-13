using System.Collections;
using UnityEngine;
using XCharts.Runtime;

namespace XCharts.Example
{
    [DisallowMultipleComponent]
    public class BarChartTest : MonoBehaviour
    {
        private BarChart chart;
        private Serie serie, serie2;
        private int m_DataNum = 5;
        private int attentionIndex = 0;


        private void OnEnable()
        {
            //StartCoroutine(PieDemo());
            chart = gameObject.GetComponent<BarChart>();
            if (chart == null)
            {
                chart = gameObject.AddComponent<BarChart>();
                chart.Init();
            }
            Debug.Log("Subscribing to MindwaveManager attention event.");

            MindwaveManager.Instance.OnAttentionReceived += OnAttentionReceived;
            InitializeChart();
        }

        private void InitializeChart()
        {
            chart.EnsureChartComponent<Title>().text = "Mindwave Attention Chart";

            var yAxis = chart.EnsureChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Default;

            chart.RemoveData();
            serie = chart.AddSerie<Bar>("Attention");

            for (int i = 0; i < m_DataNum; i++)
            {
                chart.AddXAxisData("x" + (i + 1));
                chart.AddData(0, 0); // Initialize with zero values
            }

            Debug.Log("Chart initialized with zero values.");
        }

        private void OnAttentionReceived(float attentionValue)
        {
            if (chart == null || serie == null) return;

            chart.UpdateData(0, attentionIndex, attentionValue);
            attentionIndex = (attentionIndex + 1) % m_DataNum;

            chart.RefreshChart();
        }

        IEnumerator PieDemo()
        {
            while (true)
            {
                StartCoroutine(AddSimpleBar());
                yield return new WaitForSeconds(2);
                StartCoroutine(BarMutilSerie());
                yield return new WaitForSeconds(3);
                StartCoroutine(ZebraBar());
                yield return new WaitForSeconds(3);
                StartCoroutine(SameBarAndNotStack());
                yield return new WaitForSeconds(3);
                StartCoroutine(SameBarAndStack());
                yield return new WaitForSeconds(3);
                StartCoroutine(SameBarAndPercentStack());
                yield return new WaitForSeconds(10);
            }
        }

        IEnumerator AddSimpleBar()
        {
            chart = gameObject.GetComponent<BarChart>();
            if (chart == null)
            {
                chart = gameObject.AddComponent<BarChart>();
                chart.Init();
            }
            chart.EnsureChartComponent<Title>().text = "Mindwave Chart";
            //chart.EnsureChartComponent<Title>().subText = "普通柱状图";

            var yAxis = chart.EnsureChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Default;

            chart.RemoveData();
            serie = chart.AddSerie<Bar>("Bar1");

            for (int i = 0; i < m_DataNum; i++)
            {
                chart.AddXAxisData("x" + (i + 1));
                if (i == 0)
                {
                    chart.AddData(0, 17);
                }
                if (i == 1)
                {
                    chart.AddData(1, 47);
                }
                //chart.AddData(0, UnityEngine.Random.Range(30, 90));
            }
            yield return new WaitForSeconds(1);
        }

        IEnumerator BarMutilSerie()
        {
            chart.EnsureChartComponent<Title>().subText = "Mindwave test1";

            float now = serie.barWidth - 0.35f;
            while (serie.barWidth > 0.35f)
            {
                serie.barWidth -= now * Time.deltaTime;
                chart.RefreshChart();
                yield return null;
            }

            serie2 = chart.AddSerie<Bar>("Bar2");
            serie2.lineType = LineType.Normal;
            serie2.barWidth = 0.35f;
            for (int i = 0; i < m_DataNum; i++)
            {
                chart.AddData(1, UnityEngine.Random.Range(20, 90));
            }
            yield return new WaitForSeconds(1);
        }

        IEnumerator ZebraBar()
        {
            chart.EnsureChartComponent<Title>().subText = "Mindwave Test2";
            serie.barType = BarType.Zebra;
            serie2.barType = BarType.Zebra;
            serie.barZebraWidth = serie.barZebraGap = 4;
            serie2.barZebraWidth = serie2.barZebraGap = 4;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);
        }

        IEnumerator SameBarAndNotStack()
        {
            chart.EnsureChartComponent<Title>().subText = "Mindwave test3";
            serie.barType = serie2.barType = BarType.Normal;
            serie.stack = "";
            serie2.stack = "";
            serie.barGap = -1;
            serie2.barGap = -1;
            yield return new WaitForSeconds(1);
        }

        IEnumerator SameBarAndStack()
        {
            chart.EnsureChartComponent<Title>().subText = "Mindwave Test4";
            serie.barType = serie2.barType = BarType.Normal;
            serie.stack = "samename";
            serie2.stack = "samename";
            yield return new WaitForSeconds(1);
            float now = 0.6f - serie.barWidth;
            while (serie.barWidth < 0.6f)
            {
                serie.barWidth += now * Time.deltaTime;
                serie2.barWidth += now * Time.deltaTime;
                chart.RefreshChart();
                yield return null;
            }
            serie.barWidth = serie2.barWidth;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);
        }

        IEnumerator SameBarAndPercentStack()
        {
            chart.EnsureChartComponent<Title>().subText = "Midnwave test5";
            serie.barType = serie2.barType = BarType.Normal;
            serie.stack = "samename";
            serie2.stack = "samename";

            serie.barPercentStack = true;
            if (null == serie.label)
            {
                serie.EnsureComponent<LabelStyle>();
            }
            serie.label.show = true;
            serie.label.position = LabelStyle.Position.Center;
            serie.label.textStyle.color = Color.white;
            serie.label.formatter = "{d:f0}%";

            if (null == serie2.label)
            {
                serie2.EnsureComponent<LabelStyle>();
            }
            serie2.label.show = true;
            serie2.label.position = LabelStyle.Position.Center;
            serie2.label.textStyle.color = Color.white;
            serie2.label.formatter = "{d:f0}%";
            serie2.labelDirty = true;

            chart.RefreshChart();
            yield return new WaitForSeconds(1);
        }
    }
}