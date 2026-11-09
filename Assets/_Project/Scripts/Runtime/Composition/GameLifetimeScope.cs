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

        private GameSaveController _saveController;

        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Регистрируем чистый C#-сервис кошелька (Синглтон)
            // Добавляем .AsSelf(), чтобы кошелек можно было прокинуть и как IWalletService, и как WalletService
            builder.Register<WalletService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Register<SaveService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameSaveController>(Lifetime.Singleton)
                .AsSelf();

            // 2. Регистрируем базовые Mono-компоненты
            builder.RegisterComponent(_pumpkin);
            builder.RegisterComponent(_upgradeManager);
            builder.RegisterComponent(_roomNavigationUI);

            // 3. Регистрируем остальные Mono-скрипты, чтобы VContainer вызвал в них метод Construct()
            builder.RegisterComponent(_donationController);
            builder.RegisterComponent(_growController);
            builder.RegisterComponent(_walletUI);
            builder.RegisterComponent(_storeGridUI);
            builder.RegisterComponent(_debugProgressReset);

        }
    }
}