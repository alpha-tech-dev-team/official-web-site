namespace ApplicationsApi.Documents;

public interface ITemplateStore
{
    string Load(int version);
}

public class FileTemplateStore : ITemplateStore
{
    public string Load(int version)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Templates", $"application-v{version}.txt");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Шаблон версии {version} не найден", path);

        return File.ReadAllText(path);
    }
}