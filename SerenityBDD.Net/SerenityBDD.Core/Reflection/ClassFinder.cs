using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SerenityBDD.Core.Steps;

namespace SerenityBDD.Core.Reflection
{
    public class FieldFinder
    {
        private readonly Type targetClass;

        private FieldFinder(Type targetClass)
        {
            this.targetClass = targetClass;
        }

        public static FieldFinder inClass(Type targetClass)
        {
            return new FieldFinder(targetClass);
        }


        public Optional<FieldInfo> findFieldCalled(string fieldName)
        {
            return findFieldCalled(fieldName, targetClass);
        }

        private Optional<FieldInfo> findFieldCalled(string fieldName, Type targetClass)
        {
            var fields = targetClass.GetFields();
            foreach (var field in fields)
            {
                if (field.Name.Equals(fieldName))
                {
                    return Optional.of(field);
                }
            }
            if (targetClass.BaseType != null)
            {
                return findFieldCalled(fieldName, targetClass.BaseType);
            }
            return (Optional<FieldInfo>)Optional.absent();
        }

        public Optional<FieldInfo> findFieldOfType(Type type)
        {
            return findFieldOfType(type, targetClass);
        }

        private Optional<FieldInfo> findFieldOfType(Type type, Type targetClass)
        {
            FieldInfo[] fields = targetClass.GetFields();
            foreach (var field in fields)
            {
                if (field.GetType().Equals(type))
                {
                    return Optional.of(field);
                }
            }
            if (targetClass.BaseType != null)
            {
                return findFieldOfType(type, targetClass.BaseType);
            }
            return (Optional<FieldInfo>)Optional.absent();
        }
    }

    class ClassFinder
    {

        //private readonly ClassLoader classLoader;
        private readonly Type annotation;

        private ClassFinder(object classLoader, Type annotation)
        {
            //this.classLoader = classLoader;
            this.annotation = annotation;
        }

        private ClassFinder(object classLoader) : this(classLoader, null)
        {
        }

        public static ClassFinder loadClasses()
        {
            return new ClassFinder(null);
        }

        public ClassFinder annotatedWith(Type annotation)
        {
            return new ClassFinder(null, annotation);
        }

        /**
         * Scans all classes accessible from the context class loader which belong to the given package and subpackages.
         *
         * @param packageName The base package
         * @return The classes
         */

        public IEnumerable<Type> fromPackage(string packageName)
        {

            return filtered(getClasses(packageName));
        }

        private IEnumerable<Type> filtered(IEnumerable<Type> classes)
        {

            return classes.Where(clazz => matchesConstraints(clazz));

        }

        private bool matchesConstraints(Type clazz)
        {
            if (annotation == null)
            {
                return true;
            }
            else
            {
                return (clazz.GetCustomAttribute(annotation) != null);
            }
        }

        /*
                private static ClassLoader getDefaultClassLoader()
                {
                    return Thread.currentThread().getContextClassLoader();
                }
                */

        private static bool isNotAnInnerClass(Type tgt)
        {
            return !tgt.IsNested;

        }

        /**
         * Scans all classes accessible from the context class loader which belong to the given package and subpackages.
         * Adapted from http://snippets.dzone.com/posts/show/4831 and extended to support use of JAR files
         * @param packageName The base package
         * @return The classes
         */
        public static IEnumerable<Type> getClasses(string asmPath)
        {
            try
            {

                var asm = Assembly.LoadFrom(asmPath);
                return asm.ExportedTypes.Where(t => isNotAnInnerClass(t));

            }
            catch (Exception e)
            {
                return Enumerable.Empty<Type>();
            }
        }
            
        /**
         * Recursive method used to find all classes in a given directory and subdirs. * Adapted from http://snippets.dzone.com/posts/show/4831 and extended to support use of JAR files * @param directory The base directory * @param packageName The package name for classes found inside the base directory * @return The classes * @throws ClassNotFoundException
         
        private static TreeSet<String> findClasses(URI directory, String packageName) throws Exception
        {
            final String scheme = directory.getScheme();
        final String schemeSpecificPart = directory.getSchemeSpecificPart();

        if (scheme.equals("jar") && schemeSpecificPart.contains("!")) {
            return findClassesInJar(directory);
    } else if (scheme.equals("file")) {
            return findClassesInFileSystemDirectory(directory, packageName);
}

        throw new IllegalArgumentException(
                "cannot handle URI with scheme [" + scheme + "]" +
                "; received directory=[" + directory + "], packageName=[" + packageName + "]"
        );
    }
    
        private static TreeSet<String> findClassesInJar(URI jarDirectory) throws Exception
{
    final String schemeSpecificPart = jarDirectory.getSchemeSpecificPart();

    TreeSet<String> classes = Sets.newTreeSet();

    String []
    split = schemeSpecificPart.split("!");
    URL jar = new URL(split[0]);
        try(ZipInputStream zip = new ZipInputStream(jar.openStream())) {
            ZipEntry entry;
            while ((entry = zip.getNextEntry()) != null) {
                if (entry.getName().endsWith(".class")) {
                    String className = classNameFor(entry);
                    if (isNotAnInnerClass(className)) {
                        classes.add(className);
                    }
                }
            }
        }

        return classes;
    }

    private static TreeSet<String> findClassesInFileSystemDirectory(URI fileSystemDirectory, String packageName) throws Exception
{
    TreeSet<String> classes = Sets.newTreeSet();

    File dir = new File(fileSystemDirectory);
        if (!dir.exists()) {
            return classes;
        }
        File[] files = dir.listFiles();
        for (File file : files) {
            if (file.isDirectory()) {
                classes.addAll(
                        findClassesInFileSystemDirectory(file.getAbsoluteFile().toURI(), packageName + "." + file.getName())
                );
            } else if (file.getName().endsWith(".class")) {
                classes.add(
                        packageName + '.' + file.getName().substring(0, file.getName().length() - 6)
                );
            }
        }

        return classes;
    }

    private static String classNameFor(ZipEntry entry)
{
    return entry.getName().replaceAll("[$].*", "").replaceAll("[.]class", "").replace('/', '.');
}
    */
    }
}
