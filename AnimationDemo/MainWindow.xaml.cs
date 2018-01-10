using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnimationDemo
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow:Window
	{
		Rectangle rect;//创建一个方块进行演示 
		NotTopmottPopup popup;//创建一个弹窗进行演示 
		int rectangleNum = 0;//黑方块的数量
		System.Windows.Threading.DispatcherTimer timer;//定时器
		FontFamily fontfamily;//字体
		ScoreEF scoreEF = new ScoreEF() { Score = 0 };

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			//对方块进行初始化  
			rect = new Rectangle();
			rect.Fill = new SolidColorBrush(Colors.Red);
			rect.Tag = 0;
			rect.Width = 50;
			rect.Height = 50;
			rect.RadiusX = 5;
			rect.RadiusY = 5;
			Carrier.Children.Add(rect);
			Canvas.SetLeft(rect,0);
			Canvas.SetTop(rect,0);

			//计分版初始化
			foreach(FontFamily _f in Fonts.SystemFontFamilies)
			{
				if(_f.Source == "Jokerman")
				{
					fontfamily = _f;
					break;
				}
			}
			Binding binding = new Binding()
			{
				Path = new PropertyPath("Score"),
				Source = scoreEF,
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			};
			TextBlock textblock = new TextBlock();
			textblock.VerticalAlignment = VerticalAlignment.Bottom;
			textblock.HorizontalAlignment = HorizontalAlignment.Center;
			textblock.FontSize = 20;
			textblock.FontFamily = fontfamily;
			textblock.Background = new SolidColorBrush(Colors.Silver);
			BindingOperations.SetBinding(textblock,TextBlock.TextProperty,binding);
			popup = new NotTopmottPopup();
			popup.Topmost = false;
			popup.PlacementTarget = Carrier;
			popup.Placement = PlacementMode.Top;
			popup.PopupAnimation = PopupAnimation.Fade;
			popup.Child = textblock;
			popup.IsOpen = true;
			popup.StaysOpen = true;

			//计时器初始化
			timer = new System.Windows.Threading.DispatcherTimer();
			timer.Tick += new EventHandler(OnTimedEvent);
			timer.Interval = TimeSpan.FromMilliseconds(1000);
			timer.Start();
		}

		/// <summary>
		/// 定时生成黑方块
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimedEvent(object sender,EventArgs e)
		{
			if(rectangleNum < 3)
			{
				//Rect rect = SystemParameters.WorkArea;//获取工作区大小  
				Random random = new Random();
				Rectangle rtg = new Rectangle();
				rtg.Fill = new SolidColorBrush(Colors.Black);
				rtg.Width = 50;
				rtg.Height = 50;
				rtg.RadiusX = 5;
				rtg.RadiusY = 5;
				Carrier.Children.Add(rtg);
				Canvas.SetLeft(rtg,NextDouble(random,0,Carrier.Width - rtg.Width));
				Canvas.SetTop(rtg,NextDouble(random,0,Carrier.Height - rtg.Height));
				//Canvas.SetLeft(rtg,NextDouble(random,0,0));
				//Canvas.SetTop(rtg,NextDouble(random,0,0));
				rectangleNum++;
			}
			else
			{
				if(MessageBox.Show("你的得分是:"+scoreEF.Score.ToString()+",你输啦,是否退出游戏?","提示",MessageBoxButton.YesNo,MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
				{
					Application.Current.Shutdown();
				}
				else
				{
					timer.Stop();
					timer = null;
				}
			}	
		}

		/// <summary>
		/// 点击和动画添加事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Carrier_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
		{
			if(timer != null)
			{
				timer.Stop();
			}
			Point p = e.GetPosition(Carrier);
			Storyboard storyboard = new Storyboard();//新建一个动画板  

			//添加方块X轴方向的动画  
			DoubleAnimation doubleAnimation = new DoubleAnimation(
			Canvas.GetLeft(rect),(p.X - rect.Width / 2),new Duration(TimeSpan.FromMilliseconds(500)));
			Storyboard.SetTarget(doubleAnimation,rect);
			Storyboard.SetTargetProperty(doubleAnimation,new PropertyPath("(Canvas.Left)"));
			storyboard.Children.Add(doubleAnimation);

			//添加方块Y轴方向的动画  
			doubleAnimation = new DoubleAnimation(
			Canvas.GetTop(rect),(p.Y - rect.Height / 2),new Duration(TimeSpan.FromMilliseconds(500)));
			Storyboard.SetTarget(doubleAnimation,rect);
			Storyboard.SetTargetProperty(doubleAnimation,new PropertyPath("(Canvas.Top)"));
			storyboard.Children.Add(doubleAnimation);

			//添加计分器X轴方向的动画  
			doubleAnimation = new DoubleAnimation(
			Canvas.GetLeft(rect),(p.X - rect.Width / 2),new Duration(TimeSpan.FromMilliseconds(500)));
			Storyboard.SetTarget(doubleAnimation,popup);
			Storyboard.SetTargetProperty(doubleAnimation,new PropertyPath("(Popup.HorizontalOffset)"));
			storyboard.Children.Add(doubleAnimation);

			//添加计分器Y轴方向的动画  
			doubleAnimation = new DoubleAnimation(
			Canvas.GetTop(rect),(p.Y - rect.Height / 2),new Duration(TimeSpan.FromMilliseconds(500)));
			Storyboard.SetTarget(doubleAnimation,popup);
			Storyboard.SetTargetProperty(doubleAnimation,new PropertyPath("(Popup.VerticalOffset)"));
			storyboard.Children.Add(doubleAnimation);

			if(!Resources.Contains("rectAnimation"))
			{
				Resources.Add("rectAnimation",storyboard);
			}
			storyboard.Completed += Storyboard_Completed;

			storyboard.Begin();//开始运行动画
		}

		/// <summary>
		/// 动画结束后判断是否消去方块
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Storyboard_Completed(object sender,EventArgs e)
		{
			bool isIntersect = false;
			foreach(var child in Carrier.Children)
			{
				if(child.GetType() == typeof(Rectangle))
				{
					Rectangle rt = child as Rectangle;
					if(rt.Tag == null)
					{
						isIntersect = JudgeRectangleIntersect(rect,rt);
						if(isIntersect)
						{
							Carrier.Children.Remove(rt);
							rectangleNum--;
							scoreEF.Score++;
							OnTimedEvent(sender,e);
							if(timer != null)
							{
								timer.Start();
							}
							return;
						}
					}
				}
			}
			if(timer != null)
			{
				timer.Start();
			}
		}

		/// <summary>
		/// 每一个黑方块和红方块对比
		/// </summary>
		/// <param name="RecA">红方块</param>
		/// <param name="RecB">黑方块</param>
		/// <returns></returns>
		public bool JudgeRectangleIntersect(Rectangle RecA, Rectangle RecB)
		{
			bool isIntersect = false;
			//100%重合 直接消去
			if((double)RecA.GetValue(Canvas.LeftProperty) == (double)RecB.GetValue(Canvas.LeftProperty) && (double)RecA.GetValue(Canvas.TopProperty) == (double)RecB.GetValue(Canvas.TopProperty))
			{
				return true;
			}
			//循环判断红方块的四个点哪个点在黑方块里面
			else
			{
				Dictionary<string,Point> pointDic = new Dictionary<string,Point>();
				Point LT = new Point((double)RecA.GetValue(Canvas.LeftProperty),(double)RecA.GetValue(Canvas.TopProperty));
				Point LB = new Point((double)RecA.GetValue(Canvas.LeftProperty),(double)RecA.GetValue(Canvas.TopProperty) + RecA.ActualHeight);
				Point RT = new Point((double)RecA.GetValue(Canvas.LeftProperty) + RecA.ActualWidth,(double)RecA.GetValue(Canvas.TopProperty));
				Point RB = new Point((double)RecA.GetValue(Canvas.LeftProperty) + RecA.ActualWidth,(double)RecA.GetValue(Canvas.TopProperty) + RecA.ActualHeight);
				pointDic.Add("LT",LT);
				pointDic.Add("LB",LB);
				pointDic.Add("RT",RT);
				pointDic.Add("RB",RB);

				foreach(var item in pointDic)
				{
					isIntersect = JudgePointToRectangle(item,RecB);
					if(isIntersect)
					{
						return isIntersect;
					}
				}
			}

			return isIntersect;
		}

		/// <summary>
		/// 计算红方块和黑方块的重叠面积
		/// </summary>
		/// <param name="item">包含红方块点的键值对</param>
		/// <param name="recB">黑方块</param>
		/// <returns></returns>
		private bool JudgePointToRectangle(KeyValuePair<string,Point> item,Rectangle recB)
		{
			bool isIntersect = false;

			Point PointA = item.Value;
			//判断X轴是否重叠
			if((double)recB.GetValue(Canvas.LeftProperty) < PointA.X && PointA.X < (double)recB.GetValue(Canvas.LeftProperty) + recB.ActualWidth)
			{
				//判断Y轴是否重叠
				if((double)recB.GetValue(Canvas.TopProperty) < PointA.Y && PointA.Y < (double)recB.GetValue(Canvas.TopProperty) + recB.ActualHeight)
				{
					Point PointB = new Point();
					//用红方块的点的相对面得知黑方块的点
					switch(item.Key)
					{
						case "LT":
							{
								PointB = new Point((double)recB.GetValue(Canvas.LeftProperty) + recB.ActualWidth,(double)recB.GetValue(Canvas.TopProperty) + recB.ActualHeight);
								break;
							}
						case "RT":
							{
								PointB = new Point((double)recB.GetValue(Canvas.LeftProperty),(double)recB.GetValue(Canvas.TopProperty) + recB.ActualHeight);
								break;
							}
						case "LB":
							{
								PointB = new Point((double)recB.GetValue(Canvas.LeftProperty) + recB.ActualWidth,(double)recB.GetValue(Canvas.TopProperty));
								break;
							}
						case "RB":
							{
								PointB = new Point((double)recB.GetValue(Canvas.LeftProperty),(double)recB.GetValue(Canvas.TopProperty));
								break;
							}
						default:
							{
								return false;
							}
					}
					//计算两点之间的面积
					double X = Math.Abs(PointA.X - PointB.X);
					double Y = Math.Abs(PointA.Y - PointB.Y);
					double Area = X * Y;
					double Brea = recB.ActualHeight * recB.ActualWidth;
					//对比重叠面积 如果大于80%就可以消去
					if(Area > Brea * 0.8)
					{
						isIntersect = true;
					}
				}
			}

			return isIntersect;
		}

		/// <summary>
		/// 生成设置范围内的Double的随机数
		/// eg:_random.NextDouble(1.5, 2.5)
		/// </summary>
		/// <param name="random">Random</param>
		/// <param name="miniDouble">生成随机数的最小值</param>
		/// <param name="maxiDouble">生成随机数的最大值</param>
		/// <returns>当Random等于NULL的时候返回0;</returns>
		public static double NextDouble(Random random,double miniDouble,double maxiDouble)
		{
			if(random != null)
			{
				return random.NextDouble() * (maxiDouble - miniDouble) + miniDouble;
			}
			else
			{
				return 0.0d;
			}
		}
	}

	/// <summary>
	/// 分数实体
	/// </summary>
	public class ScoreEF:INotifyPropertyChanged
	{
		private int score;
		public int Score
		{
			get
			{
				return score;
			}
			set
			{
				score = value;
				OnChangedProperties("Score");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnChangedProperties(string propertyName)
		{
			this.PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
		}
	}

	public class NotTopmottPopup:Popup
	{
		//创造依赖项属性
		public static DependencyProperty TopmostProperty = Window.TopmostProperty.AddOwner(typeof(NotTopmottPopup),new FrameworkPropertyMetadata(false,OnTopmostChanged));
		public bool Topmost
		{
			get
			{
				return (bool)GetValue(TopmostProperty);
			}
			set
			{
				SetValue(TopmostProperty,value);
			}	
		}
		//topmost值改变时要更新窗口
		private static void OnTopmostChanged(DependencyObject obj,DependencyPropertyChangedEventArgs e)
		{
			(obj as NotTopmottPopup).UpdateWindow();
		}
		//重写popup打开时的窗口事件
		protected override void OnOpened(EventArgs e)
		{
			UpdateWindow();
		}
		private void UpdateWindow()
		{
			//获取popup句柄
			var hwnd = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
			RECT rect;
			if(GetWindowRect(hwnd,out rect))
			{
				//根据topmost的值设置是否置顶
				SetWindowPos(hwnd,Topmost ? -1 : -2,rect.Left,rect.Top,(int)this.Width,(int)this.Height,0);
			}
		}
		#region DLL引用的接口和定义
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hWnd,out RECT lpRect);
		[DllImport("user32",EntryPoint = "SetWindowPos")]
		private static extern int SetWindowPos(IntPtr hWnd,int hwndInsertAfter,int x,int y,int cx,int cy,int wFlags);
		#endregion
	}
}
