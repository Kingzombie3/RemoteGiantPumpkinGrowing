using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Features.Farm;
using _Project.Scripts.Runtime.Infrastructure.SaveSystem;
using _Project.Scripts.Runtime.Features.Upgrades;
using _Project.Scripts.Runtime.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using _Project.Scripts.Runtime.Features.SaveSystem;
using _Project.Scripts.Runtime.UI.DebugTools;
using _Project.Scripts.Runtime.UI.Effects;

namespace _Project.Scripts.Runtime.Composition
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private Pumpkin _pumpkin;
        [SerializeField] private UpgradeManager _upgradeManager;
        [SerializeField] private RoomNavigationUI _roomNavigationUI;

        [Header("[Inject]-Needed")]
        [SerializeField] private DonationController _donationController;
        [SerializeField] private GrowController _growController;
        [SerializeField] private WalletUI _walletUI;
        [SerializeField] private StoreGridUI _storeGridUI;
        [SerializeField] private DebugProgressReset _debugProgressReset;
        [SerializeField] private HeatHazeController _heatHazeController;
        [SerializeField] private WeatherController _weatherController;
        [SerializeField] private RainWeatherEffect _rainWeatherEffect;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<WalletService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<SaveService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameSaveController>(Lifetime.Singleton)
                .AsSelf();

            // Регистрируем базовые Mono-компоненты
            builder.RegisterComponent(_pumpkin);
            builder.RegisterComponent(_upgradeManager);
            builder.RegisterComponent(_roomNavigationUI);

            // Регистрируем остальные Mono-скрипты, чтобы VContainer вызвал в них метод Construct()
            builder.RegisterComponent(_donationController);
            builder.RegisterComponent(_growController);
            builder.RegisterComponent(_walletUI);
            builder.RegisterComponent(_storeGridUI);
            builder.RegisterComponent(_debugProgressReset);
            builder.RegisterComponent(_heatHazeController);
            builder.RegisterComponent(_weatherController);
            builder.RegisterComponent(_rainWeatherEffect);

        }
    }
}